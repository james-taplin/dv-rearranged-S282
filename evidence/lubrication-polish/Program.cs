using System;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

// Executes the production surface-intersection function against the local
// native rod export. Does not emulate Unity animation or claim a game test.
class Program
{
    static void Main(string[] args)
    {
        using var data = JsonDocument.Parse(System.IO.File.ReadAllText(args[0]));
        var mesh = data.RootElement.GetProperty("meshes").EnumerateArray().Single(m => m.GetProperty("name").GetString() == "native");
        var vertices = mesh.GetProperty("vertices").EnumerateArray().Select(v => new Vector3(v[0].GetSingle(), v[1].GetSingle(), v[2].GetSingle())).ToArray();
        var triangles = mesh.GetProperty("triangles").EnumerateArray().Select(i => i.GetInt32()).ToArray();
        int checks = 0;
        void Check(bool ok, string why) { if (!ok) throw new Exception(why); checks++; }
        var seats = new System.Collections.Generic.List<object>();
        // Three additional cups per side; the main-rod cup remains native.
        foreach (float z in new[] { 1.7f, 0f, -3.1f })
        {
            Check(!OilCupSeat.TryTop(vertices, triangles, .156308f, z, out _), "Old main-pin X unexpectedly sits on coupling rod");
            Check(OilCupSeat.TryTop(vertices, triangles, 0f, z, out var y), "No bearing surface at " + z);
            Check(y > .04f && y < .13f, "Surface outside native bearing");
            foreach (float scale in new[] { .8f, 1f, 1.11f, 1.198f })
                foreach (int side in new[] { -1, 1 })
                {
                    // Test mirrored/nonuniform-scaled copies of the native mesh.
                    var transformed = vertices.Select(v => new Vector3(side * v.x, scale * v.y, scale * v.z)).ToArray();
                    Check(OilCupSeat.TryTop(transformed, triangles, 0f, z * scale, out var worldY), "Scaled seat missing");
                    Check(Math.Abs(worldY - y * scale) < .00001f, "Scaled seat drift");
                }
            seats.Add(new { z, y, oldLateralGapAtLeastM = .156308f - vertices.Max(v => v.x) });
        }
        Check(!OilCupSeat.TryTop(vertices, triangles, 0f, 20f, out _), "Out-of-range mount accepted");
        Check(!OilCupSeat.TryTop(new[] { new Vector3(), new Vector3(), new Vector3() }, new[] { 0, 1, 2 }, 0, 0, out _), "Degenerate triangle accepted");
        int lubricatorChecks = WheelRearranger.TestLubricator();
        int couplingSeatChecks = CouplingSeats.Run(args[0]);
        int malletPitchChecks = WheelRearranger.TestMalletPitch(args[1]);
        Console.WriteLine(JsonSerializer.Serialize(new { passed = true, checks, lubricatorChecks, couplingSeatChecks, malletPitchChecks, seats, runtimeValidated = false }));
    }
}
