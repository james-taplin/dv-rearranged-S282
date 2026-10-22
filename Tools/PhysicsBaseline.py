"""Compare a captured baseline with the actual native-engine validation output.

No game files are changed. Uses matplotlib, with a Pillow fallback when unavailable.
"""
import argparse
import csv
import json
import math
from pathlib import Path


def build(capture_path, validation_path, output):
    capture = json.loads(Path(capture_path).read_text(encoding="utf-8-sig"))
    validation = json.loads(Path(validation_path).read_text(encoding="utf-8-sig"))
    assert capture["passed"] and validation["passed"]
    proposed = {r["id"]: r for r in validation["table"]}
    assert len(capture["arrangements"]) == 47 and len({a["id"] for a in capture["arrangements"]}) == 47
    # The same 15 bar absolute / .75 cutoff / 300 C native mean reference.
    k = .75 * (1 + math.log(1 / .75))
    mean_to_nominal = (15 * k - 1) / (.85 * 14)
    rows = []
    for a in capture["arrangements"]:
        m, ident = a["measurements"], a["id"]
        old_mean = m["referenceMeanForceN"] / 1000
        old_nominal = old_mean / mean_to_nominal
        grip = m["referenceDryGripN"] / 1000
        n = proposed.get(ident)
        row = dict(id=ident, arrangement=a["name"], old_nominal_kN=old_nominal,
                   new_nominal_kN=n["nominalPullKN"] if n else old_nominal,
                   new_reference_mean_kN=n["referencePullKN"] if n else old_mean,
                   rated_dry_grip_kN=n["referenceGripKN"] if n else grip,
                   rated_adhesive_t=n["ratedAdhesiveMassKg"] / 1000 if n else None,
                   nominal_factor_of_adhesion=n["factorOfAdhesion"] if n else None,
                   driver_diameter_m=m["driverRadiusM"] * 2,
                   status="recalibrated" if n else "stock unchanged" if ident == 10 else "prototype excluded" if ident == 36 else "unpowered")
        row["change_percent"] = 100 * (row["new_nominal_kN"] / old_nominal - 1) if old_nominal else 0
        rows.append(row)
    # Erie third engine is a separate traction group, never double-counted.
    erie = next(a for a in capture["arrangements"] if a["id"] == 43)
    tender = next(e for e in erie["measurements"]["engines"] if e["tractionGroup"] == "tender")
    tender_radius = erie["measurements"]["tender"]["driverRadiusM"]
    old_tender = tender["cylinders"] * tender["boreM"] ** 2 * tender["strokeM"] * (.85 * 14e5) / (4 * tender_radius) / 1000
    tender_nominal = proposed[43]["tenderNominalPullKN"]
    rows.append(dict(id="43T", arrangement="Erie tender engine (separate)", old_nominal_kN=old_tender,
                     new_nominal_kN=tender_nominal, new_reference_mean_kN=tender_nominal * mean_to_nominal,
                     rated_dry_grip_kN=50 * .8 * 9.81 * .24, rated_adhesive_t=40,
                     nominal_factor_of_adhesion=40 * 9.81 / tender_nominal,
                     driver_diameter_m=erie["measurements"]["tender"]["driverRadiusM"] * 2,
                     status="tender independent design", change_percent=100 * (tender_nominal / old_tender - 1)))
    output = Path(output)
    output.mkdir(parents=True, exist_ok=True)
    (output / "baseline.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
    with (output / "baseline.csv").open("w", newline="", encoding="utf-8-sig") as f:
        w = csv.DictWriter(f, fieldnames=list(rows[0]))
        w.writeheader()
        w.writerows(rows)
    lines = ["# Calculated physics baseline: 1.20 capture versus 1.21 design", "",
             "Locomotive rated mass 124 t; tender design mass 50 t. Native dry coefficient .24.",
             "Nominal TE: 85% of 14 bar gauge. Native mean reference: 15 bar absolute, .75 cutoff, 300 C.",
             "Design minimum nominal factor of adhesion 4.4. Original cylinder capacity is an upper bound.",
             "Stock remains untouched. Effective loads are explicit gameplay assumptions, not reconstructed spring reactions.", "",
             "| Arrangement | Adhesive t | Previous nominal kN | New nominal kN | Dry grip kN | Change |",
             "|---|---:|---:|---:|---:|---:|"]
    for r in rows:
        load = f'{r["rated_adhesive_t"]:.1f}' if r["rated_adhesive_t"] is not None else "n/a"
        lines.append(f'| {r["arrangement"]} | {load} | {r["old_nominal_kN"]:.1f} | {r["new_nominal_kN"]:.1f} | {r["rated_dry_grip_kN"]:.1f} | {r["change_percent"]:+.1f}% |')
    lines += ["", "The Erie locomotive and tender rows are separate. Do not add their grip to another row twice.",
              "These are nominal cylinder and adhesion comparisons, not measured drawbar pull, a power curve, top speed, or a tonnage rating.",
              "A single S282 boiler is retained; extra cylinders do not create extra sustained boiler power.",
              "Reference dimensions are fixed. Actual grip follows mass and the native rail/sanding state; it can differ from rated grip.",
              "", "Sources and rationale: PHYSICS_BASELINE.md in the repository."]
    (output / "baseline.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    try:
        import matplotlib
    except ImportError:
        from PIL import Image, ImageDraw, ImageFont
        def fallback(data, name):
            width, top, step, left, right = 1900, 190, 52, 600, 120
            height = top + len(data) * step + 130
            im = Image.new("RGB", (width, height), "#ffffff")
            d = ImageDraw.Draw(im)
            def font(size):
                return ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", size)
            maximum = math.ceil(max(max(r["old_nominal_kN"], r["rated_dry_grip_kN"]) for r in data) / 50) * 50
            def x(v):
                return left + v / maximum * (width - left - right)
            d.text((35, 25), "Calculated pull and grip: 1.20 versus 1.21", font=font(35), fill="#19344a")
            d.text((35, 78), "Rated 124 t locomotive / 50 t tender | stock unchanged | nominal TE at 85% of 14 bar gauge", font=font(23), fill="#445566")
            for v in range(0, int(maximum) + 1, 50):
                d.line((x(v), top - 10, x(v), height - 95), fill="#e5e9ed", width=2)
                d.text((x(v) - 15, height - 80), str(v), font=font(20), fill="#445566")
            for i, r in enumerate(data):
                y = top + i * step
                d.text((35, y + 3), r["arrangement"], font=font(22), fill="#19344a")
                d.rectangle((left, y, x(r["old_nominal_kN"]), y + 15), fill="#9aa7b5")
                d.rectangle((left, y + 19, x(r["new_nominal_kN"]), y + 34), fill="#238577")
                d.line((x(r["rated_dry_grip_kN"]), y - 2, x(r["rated_dry_grip_kN"]), y + 38), fill="#c06424", width=4)
                d.text((width - right + 12, y + 3), f'{r["change_percent"]:+.0f}%', font=font(21), fill="#445566")
            for xx, color, label in ((35, "#9aa7b5", "1.20 nominal TE"), (415, "#238577", "1.21 nominal TE"), (795, "#c06424", "Rated dry grip")):
                d.rectangle((xx, 132, xx + 28, 151), fill=color)
                d.text((xx + 40, 123), label, font=font(23), fill="#445566")
            d.text((left + 350, height - 48), "Wheel-rim force (kN)", font=font(21), fill="#445566")
            d.text((35, height - 25), "Calculated reference comparison; not sustained power, top speed or drawbar ratings. Erie tender shown separately.", font=font(17), fill="#445566")
            im.save(output / (name + ".png"))
        fallback([r for r in rows if r["id"] != 36], "all-arrangements")
        fallback([r for r in rows if r["id"] in (10, 0, 34, 44, 9, 39, 46, 43, "43T")], "selected-arrangements")
        close = [r["arrangement"] for r in rows if isinstance(r["id"], int) and r["id"] in proposed and abs(r["change_percent"]) < 5]
        print(json.dumps(dict(arrangements=len(rows), within_five_percent=close, output=str(output)), indent=2))
        return
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    def plot(data, name):
        fig, ax = plt.subplots(figsize=(13, max(4.5, .38 * len(data) + 2)))
        y = list(range(len(data)))
        ax.barh([v - .18 for v in y], [r["old_nominal_kN"] for r in data], height=.32, color="#9aa7b5", label="1.20 nominal cylinder TE")
        ax.barh([v + .18 for v in y], [r["new_nominal_kN"] for r in data], height=.32, color="#238577", label="1.21 nominal cylinder TE")
        ax.scatter([r["rated_dry_grip_kN"] for r in data], y, marker="|", s=180, color="#c06424", label="Rated dry grip")
        ax.set_yticks(y, [r["arrangement"] for r in data], fontsize=9)
        ax.invert_yaxis()
        ax.set_xlabel("Force at wheel rim (kN)")
        ax.set_title("Calculated pull and grip baseline\n124 t locomotive / 50 t tender; stock unchanged", loc="left", pad=36)
        ax.legend(loc="lower left", bbox_to_anchor=(0, 1.005), ncols=3, fontsize=8)
        ax.set_axisbelow(True)
        ax.grid(axis="x", alpha=.2)
        ax.spines[["top", "right"]].set_visible(False)
        fig.text(.02, .008, "Nominal TE at 85% of 14 bar gauge. Separate Erie tender row. Not sustained power, speed or drawbar ratings.", fontsize=8)
        fig.tight_layout(rect=(0, .025, 1, 1))
        fig.savefig(output / (name + ".png"), dpi=160)
        fig.savefig(output / (name + ".svg"))
        plt.close(fig)
    plot([r for r in rows if r["id"] != 36], "all-arrangements")
    plot([r for r in rows if r["id"] in (10, 0, 34, 44, 9, 39, 46, 43, "43T")], "selected-arrangements")
    close = [r["arrangement"] for r in rows if isinstance(r["id"], int) and r["id"] in proposed and abs(r["change_percent"]) < 5]
    print(json.dumps(dict(arrangements=len(rows), within_five_percent=close, output=str(output)), indent=2))


if __name__ == "__main__":
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("capture")
    p.add_argument("validation")
    p.add_argument("output")
    a = p.parse_args()
    build(a.capture, a.validation, a.output)
