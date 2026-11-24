using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public static class MeasureBaselineFps
{
    public static async Task<string> Run()
    {
        if (!EditorApplication.isPlaying)
        {
            return "ERROR: Editor tidak sedang di Play Mode.";
        }

        // Tunggu 15 frame warm-up setelah masuk Play Mode agar hasil tidak tercampur lonjakan awal
        int startFrame = Time.frameCount;
        while (Time.frameCount < startFrame + 15)
        {
            await Task.Yield();
        }

        // Ukur 90 frame beruntun untuk mendapat FPS rata-rata, minimum, maksimum, dan statistik render
        const int sampleFrames = 90;
        float totalTime = 0f;
        float minDt = float.MaxValue;
        float maxDt = 0f;
        int measured = 0;
        int lastFrame = Time.frameCount;

        while (measured < sampleFrames)
        {
            await Task.Yield();
            if (Time.frameCount != lastFrame)
            {
                lastFrame = Time.frameCount;
                float dt = Time.unscaledDeltaTime;
                if (dt > 0.00001f)
                {
                    totalTime += dt;
                    if (dt < minDt) minDt = dt;
                    if (dt > maxDt) maxDt = dt;
                    measured++;
                }
            }
        }

        float avgFps = measured / totalTime;
        float maxFps = 1f / minDt;
        float minFps = 1f / maxDt;
        int drawCalls = UnityStats.drawCalls;
        int triangles = UnityStats.triangles;
        int vertices = UnityStats.vertices;
        float allocMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        float reservedMb = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);

        return $"Frames={measured} | AvgFPS={avgFps:0.0} | MinFPS={minFps:0.0} | MaxFPS={maxFps:0.0} | DrawCalls={drawCalls} | Tris={triangles} | Verts={vertices} | AllocMem={allocMb:0.1}MB | ReservedMem={reservedMb:0.1}MB";
    }
}
