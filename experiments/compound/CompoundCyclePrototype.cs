// Offline prototype adapted from James Taplin's Rearranged S060 Meyer cycle.
// Existing project MIT notices apply. Excluded from the shipping runtime project.
using System;

namespace RearrangedS282.Experiments
{
    // A finite-volume, ideal-vapour compound model for DV. SI units throughout.
    // This is a gameplay approximation (constant gamma, no two-phase receiver),
    // not an IF97 steam plant model. Unlike two parallel native engines, LP
    // receives only conserved HP exhaust and HP works against receiver pressure.
    internal sealed class CompoundCyclePrototype
    {
        internal const double GasConstant = 461.5, Gamma = 4.0 / 3.0;
        internal const double Cv = GasConstant / (Gamma - 1), Cp = Cv + GasConstant;
        internal const double Atmosphere = 100000;

        internal struct Parcel
        {
            internal double Mass, Energy;
            internal Parcel(double mass, double energy) { Mass = mass; Energy = energy; }
        }

        internal sealed class Vessel
        {
            internal readonly double Volume;
            internal double Mass { get; private set; }
            internal double Energy { get; private set; }
            internal double Pressure => (Gamma - 1) * Energy / Volume;
            internal double Temperature => Mass > 1e-12 ? Energy / (Mass * Cv) : 298.15;
            internal Vessel(double volume) { if (!Finite(volume) || !(volume > 0)) throw new ArgumentOutOfRangeException(nameof(volume)); Volume = volume; }
            internal void Add(Parcel parcel) { Mass += parcel.Mass; Energy += parcel.Energy; }
            internal Parcel Take(double requested)
            {
                double removed = Math.Min(Math.Max(0, requested), Mass);
                if (removed == 0) return default(Parcel);
                // Adiabatic blowdown of a well-mixed rigid vessel. The parcel
                // carries the exact energy lost; transferring it cannot create heat.
                double remaining = Mass - removed;
                double nextEnergy = remaining > 1e-12 ? Energy * Math.Pow(remaining / Mass, Gamma) : 0;
                var parcel = new Parcel(removed, Energy - nextEnergy);
                Mass = remaining; Energy = nextEnergy;
                return parcel;
            }
            internal double Cool(double seconds)
            {
                double heat = Math.Min(Energy, Math.Max(0, Temperature - 298.15) * 4 * seconds);
                Energy -= heat; return heat;
            }
            internal void Restore(double mass, double energy)
            {
                if (!Finite(mass) || !Finite(energy) || mass < 0 || energy < 0 || mass > 100 ||
                    energy > 1e9 || (mass == 0 && energy != 0) || (mass > 0 && energy / (mass * Cv) > 2500))
                    throw new ArgumentException("Invalid saved Meyer vapour vessel.");
                Mass = mass; Energy = energy;
            }
        }

        internal sealed class Stage
        {
            internal readonly Vessel Chest;
            internal readonly double SweptPerRevolution, MaximumIntake;
            internal double Torque, Intake, Exhaust, Vent, VentEnergy, ExhaustTemperature, AdmissionPressure;
            internal Stage(double bore, double stroke, double chestCubicMetres, double maximumIntake)
            {
                if (!Finite(bore) || !Finite(stroke) || !Finite(maximumIntake) || !(bore > 0 && stroke > 0 && maximumIntake > 0)) throw new ArgumentException("Invalid Meyer stage calibration.");
                Chest = new Vessel(chestCubicMetres);
                SweptPerRevolution = 4 * Math.PI * bore * bore / 4 * stroke; // two double-acting cylinders
                MaximumIntake = maximumIntake;
            }
            internal void Clear() { Torque = Intake = Exhaust = Vent = VentEnergy = 0; }
        }

        internal readonly Stage Hp;
        internal readonly Stage[] LowPressure;
        internal Stage Lp => LowPressure[0];
        internal readonly Vessel Receiver;
        private readonly double[] demands;
        private readonly bool enforceTransferPressure;
        internal double BoilerMass, BoilerEnergy, AtmosphereMass, AtmosphereEnergy, Work, HeatLoss;
        internal double MaximumTransferPressureExcessPa { get; private set; }
        internal double StoredMass { get { double value = Hp.Chest.Mass + Receiver.Mass; foreach (var lp in LowPressure) value += lp.Chest.Mass; return value; } }
        internal double StoredEnergy { get { double value = Hp.Chest.Energy + Receiver.Energy; foreach (var lp in LowPressure) value += lp.Chest.Energy; return value; } }
        internal double MassResidual => BoilerMass - AtmosphereMass - StoredMass;
        internal double EnergyResidual => BoilerEnergy - AtmosphereEnergy - Work - HeatLoss - StoredEnergy;

        internal CompoundCyclePrototype(double hpBore, double lpBore, double stroke, double hpChest,
            double lpChest, double flow, double receiverVolume, int lpEngines = 1, bool enforceTransferPressure = true)
        {
            if (lpEngines < 1 || lpEngines > 2) throw new ArgumentOutOfRangeException(nameof(lpEngines));
            this.enforceTransferPressure = enforceTransferPressure;
            Hp = new Stage(hpBore, stroke, hpChest, flow); Receiver = new Vessel(receiverVolume);
            LowPressure = new Stage[lpEngines]; demands = new double[lpEngines];
            for (int i = 0; i < lpEngines; i++) LowPressure[i] = new Stage(lpBore, stroke, lpChest, flow * 2);
        }

        internal void Advance(double seconds, double boilerBarAbsolute, double boilerCelsius,
            double regulator, double reverser, double hpRpm, double[] lpRpm, double cocks, bool hpCracked, bool lpCracked,
            double minimumCutoff, double maximumCutoff, double availableBoilerMass = double.PositiveInfinity)
        {
            if (!Finite(seconds) || seconds <= 0 || seconds > 1 || !Finite(boilerBarAbsolute) ||
                !Finite(boilerCelsius) || !Finite(regulator) || !Finite(reverser) || !Finite(hpRpm) || !Finite(cocks) ||
                lpRpm == null || lpRpm.Length != LowPressure.Length ||
                !Finite(minimumCutoff) || !Finite(maximumCutoff) || minimumCutoff < 0 || maximumCutoff > 1 || minimumCutoff > maximumCutoff ||
                double.IsNaN(availableBoilerMass) || availableBoilerMass < 0)
                throw new ArgumentException("Invalid Meyer compound input.");
            for (int i = 0; i < lpRpm.Length; i++) if (!Finite(lpRpm[i])) throw new ArgumentException("Invalid LP crank speed.");
            Hp.Clear(); foreach (var lp in LowPressure) lp.Clear();
            double remaining = seconds;
            while (remaining > 1e-10)
            {
                double dt = Math.Min(.005, remaining); remaining -= dt;
                double supplyP = Math.Max(Atmosphere, boilerBarAbsolute * 100000);
                double supplyT = Math.Max(298.15, Math.Min(1500, boilerCelsius + 273.15));
                double requested = IntakeMass(supplyP, Hp.Chest.Pressure, Hp.MaximumIntake * Clamp01(regulator), dt);
                // Pressure-limited filling; source carries enthalpy, not an
                // arbitrary second boiler/pressure multiplier for the LP stage.
                double pressureRoom = Math.Max(0, supplyP * Hp.Chest.Volume / (Gamma - 1) - Hp.Chest.Energy);
                double mass = Math.Min(availableBoilerMass, Math.Min(requested, pressureRoom / (Cp * supplyT)));
                availableBoilerMass -= mass;
                var fresh = new Parcel(mass, mass * Cp * supplyT);
                Hp.Chest.Add(fresh); Hp.Intake += mass; BoilerMass += mass; BoilerEnergy += fresh.Energy;

                double cutoff = minimumCutoff + (maximumCutoff - minimumCutoff) * Clamp01(Math.Abs(reverser));
                double sign = reverser < 0 ? -1 : 1;
                var hpExhaust = Expand(Hp, Math.Max(Atmosphere, Receiver.Pressure), cutoff, hpRpm, sign, dt, Receiver);
                Receiver.Add(hpExhaust);
                if (hpExhaust.Mass > 0) MaximumTransferPressureExcessPa = Math.Max(MaximumTransferPressureExcessPa, Receiver.Pressure - Hp.Chest.Pressure);
                Vent(Hp, cocks, hpCracked, dt);

                // Compute all demands from the same receiver state, then
                // share one parcel. Erie LP consumers cannot each spend the
                // complete HP exhaust or receive priority from update order.
                double totalDemand = 0;
                for (int i = 0; i < LowPressure.Length; i++)
                {
                    var lp = LowPressure[i];
                    double requestedLp = IntakeMass(Receiver.Pressure, lp.Chest.Pressure, lp.MaximumIntake, dt);
                    double room = Math.Max(0, Receiver.Pressure * lp.Chest.Volume / (Gamma - 1) - lp.Chest.Energy);
                    demands[i] = Math.Min(requestedLp, room / Math.Max(1, Cp * Receiver.Temperature));
                    totalDemand += demands[i];
                }
                double transfer = Math.Min(totalDemand, Receiver.Mass * .1);
                if (enforceTransferPressure && transfer > 0)
                {
                    bool FitsRecipients(double removed)
                    {
                        double nextEnergy = Receiver.Energy * Math.Pow((Receiver.Mass - removed) / Receiver.Mass, Gamma);
                        double receiverPressure = (Gamma - 1) * nextEnergy / Receiver.Volume;
                        for (int i = 0; i < LowPressure.Length; i++)
                        {
                            if (demands[i] <= 0) continue;
                            double energy = LowPressure[i].Chest.Energy + (Receiver.Energy - nextEnergy) * demands[i] / totalDemand;
                            if ((Gamma - 1) * energy / LowPressure[i].Chest.Volume > receiverPressure) return false;
                        }
                        return true;
                    }
                    if (!FitsRecipients(transfer))
                    {
                        double low = 0, high = transfer;
                        for (int i = 0; i < 44; i++)
                        { double mid = (low + high) * .5; if (FitsRecipients(mid)) low = mid; else high = mid; }
                        transfer = low;
                    }
                }
                var shared = Receiver.Take(transfer);
                for (int i = 0; i < LowPressure.Length; i++)
                {
                    var lp = LowPressure[i];
                    double share = totalDemand > 0 ? demands[i] / totalDemand : 0;
                    lp.Chest.Add(new Parcel(shared.Mass * share, shared.Energy * share)); lp.Intake += shared.Mass * share;
                    if (share * shared.Mass > 0) MaximumTransferPressureExcessPa = Math.Max(MaximumTransferPressureExcessPa, lp.Chest.Pressure - Receiver.Pressure);
                    var tail = Expand(lp, Atmosphere, cutoff, lpRpm[i], sign, dt);
                    AtmosphereMass += tail.Mass; AtmosphereEnergy += tail.Energy;
                    Vent(lp, cocks, lpCracked, dt);
                }
                HeatLoss += Receiver.Cool(dt);
            }
            Normalize(Hp, seconds); foreach (var lp in LowPressure) Normalize(lp, seconds);
        }
        private static void Normalize(Stage stage, double seconds)
        {
            stage.Torque /= seconds; stage.Intake /= seconds;
            stage.Exhaust /= seconds; stage.Vent /= seconds;
        }

        private Parcel Expand(Stage stage, double backPressure, double cutoff, double rpm, double direction, double dt, Vessel destination = null)
        {
            double pressure = stage.Chest.Pressure;
            stage.AdmissionPressure = pressure;
            if (stage.Chest.Mass <= 1e-12 || pressure <= backPressure || cutoff <= 0) return default(Parcel);
            double cut = Math.Max(.001, Math.Min(1, cutoff));
            // Integral of P dV over admission then polytropic expansion, minus
            // the actual exhaust back-pressure work over the complete stroke.
            double integral = cut + (Math.Pow(cut, Gamma) - cut) / (1 - Gamma);
            double workPerRev = Math.Max(0, (pressure * integral - backPressure) * stage.SweptPerRevolution);
            double revs = Math.Abs(rpm) / 60;
            double wanted = stage.Chest.Mass / stage.Chest.Volume * stage.SweptPerRevolution * cut * revs * dt;
            double admitted = Math.Min(wanted, stage.Chest.Mass * .1);
            if (enforceTransferPressure && destination != null && admitted > 0)
            {
                // Bound the actual outgoing mass/energy parcel by the end-of-
                // transfer pressure gradient. Capping receiver pressure AFTER
                // addition would discard conserved energy; transfer less instead.
                double oldMass = stage.Chest.Mass, oldEnergy = stage.Chest.Energy;
                bool Fits(double removed)
                {
                    double remaining = oldMass - removed;
                    double nextEnergy = remaining > 1e-12 ? oldEnergy * Math.Pow(remaining / oldMass, Gamma) : 0;
                    double parcelEnergy = oldEnergy - nextEnergy;
                    double share = wanted > 1e-12 ? removed / wanted : 1;
                    double desiredWork = workPerRev * revs * dt * share;
                    double availableWork = parcelEnergy * (1 - Math.Pow(backPressure / pressure, (Gamma - 1) / Gamma));
                    double work = Math.Min(desiredWork, Math.Max(0, availableWork));
                    double nextReceiverPressure = (Gamma - 1) * (destination.Energy + parcelEnergy - work) / destination.Volume;
                    return nextReceiverPressure <= (Gamma - 1) * nextEnergy / stage.Chest.Volume;
                }
                if (!Fits(admitted))
                {
                    double low = 0, high = admitted;
                    for (int i = 0; i < 44; i++)
                    { double middle = (low + high) * .5; if (Fits(middle)) low = middle; else high = middle; }
                    admitted = low;
                }
            }
            var packet = stage.Chest.Take(admitted);
            double fraction = wanted > 1e-12 ? packet.Mass / wanted : 1;
            double targetWork = workPerRev * revs * dt * fraction;
            // No work may exceed the isentropic enthalpy drop of the steam
            // actually discharged. Leave the remaining energy in HP exhaust.
            double available = packet.Energy * (1 - Math.Pow(backPressure / pressure, (Gamma - 1) / Gamma));
            double work = Math.Min(targetWork, Math.Max(0, available));
            double torque = revs > 1e-9 ? work / (2 * Math.PI * revs * dt) : workPerRev / (2 * Math.PI);
            stage.Torque += direction * torque * dt;
            stage.Exhaust += packet.Mass;
            packet.Energy -= work; Work += work;
            stage.ExhaustTemperature = packet.Mass > 1e-12 ? packet.Energy / (packet.Mass * Cp) - 273.15 : 25;
            return packet;
        }

        private void Vent(Stage stage, double cocks, bool cracked, double dt)
        {
            double control = Clamp01(cocks) + (cracked ? 5 : 0);
            double mass = .004 * control * Math.Max(0, stage.Chest.Pressure / 100000 - 1) * dt;
            var packet = stage.Chest.Take(Math.Min(mass, stage.Chest.Mass * .1));
            stage.Vent += packet.Mass; stage.VentEnergy += packet.Energy;
            AtmosphereMass += packet.Mass; AtmosphereEnergy += packet.Energy;
        }

        private static double IntakeMass(double source, double destination, double capacity, double dt) =>
            source <= Math.Max(Atmosphere, destination) ? 0 : capacity * Math.Sqrt((source - Math.Max(Atmosphere, destination)) / source) * dt;
        internal static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static double Clamp01(double value) => Math.Max(0, Math.Min(1, value));
    }
}
