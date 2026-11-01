using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Defense.Editor.P03
{
    /// <summary>
    /// P03 샘플 씬의 동작을 플레이 모드 없이 프레임 PNG 로 남긴다 — 클립을 시간으로 샘플링(`AnimationClip.SampleAnimation`)하고
    /// 카메라를 RenderTexture 로 렌더한다. 슬라임은 Idle · Hop 을 번갈아 맡고 개체마다 위상이 다르며, 박쥐는 Flap.
    /// </summary>
    public static class P03FrameRecorder
    {
        private const string IDLE_CLIP = "Idle";
        private const string HOP_CLIP = "Hop";
        private const string FLAP_CLIP = "Flap";
        private const int DEPTH_BITS = 24;
        private const int MSAA_SAMPLES = 4;

        /// <summary>
        /// 씬을 열어 frameCount 장을 fps 간격으로 outDir 에 `frame_000.png` 부터 쓰고, 끝나면 씬을 디스크에서 다시 연다(샘플링한 자세를 버린다).
        /// </summary>
        public static string Record(string outDir, int frameCount, int fps, int width, int height)
        {
            Scene scene = EditorSceneManager.OpenScene(P03SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            Transform root = GetRoot(scene);
            Camera camera = root.Find(P03SampleSceneBuilder.CAMERA_NAME).GetComponent<Camera>();
            Transform slimes = root.Find(P03SampleSceneBuilder.SLIMES_NAME);
            Transform bats = root.Find(P03SampleSceneBuilder.BATS_NAME);
            Dictionary<string, AnimationClip> slimeClips = LoadClips(P03SampleSceneBuilder.SLIME_MODEL_PATH);
            Dictionary<string, AnimationClip> batClips = LoadClips(P03SampleSceneBuilder.BAT_MODEL_PATH);
            AnimationClip flap = batClips[FLAP_CLIP];

            Directory.CreateDirectory(outDir);
            for (int frame = 0; frame < frameCount; ++frame)
            {
                float time = (float)frame / fps;
                for (int i = 0; i < slimes.childCount; ++i)
                {
                    AnimationClip clip = slimeClips[i % 2 == 0 ? IDLE_CLIP : HOP_CLIP];
                    float phase = clip.length * i / slimes.childCount;
                    clip.SampleAnimation(slimes.GetChild(i).gameObject, Mathf.Repeat(time + phase, clip.length));
                }
                for (int i = 0; i < bats.childCount; ++i)
                {
                    float phase = flap.length * i / bats.childCount;
                    flap.SampleAnimation(bats.GetChild(i).gameObject, Mathf.Repeat(time + phase, flap.length));
                }
                RenderToPng(camera, Path.Combine(outDir, "frame_" + frame.ToString("000") + ".png"), width, height);
            }

            // 다시 열면 지금 씬의 오브젝트가 파괴된다 — 요약을 먼저 만든다
            string summary = outDir + " frames=" + frameCount + " fps=" + fps + " size=" + width + "x" + height
                + " slimes=" + slimes.childCount + " bats=" + bats.childCount
                + " idle/hop/flap length=" + slimeClips[IDLE_CLIP].length.ToString("0.###") + "/" + slimeClips[HOP_CLIP].length.ToString("0.###") + "/" + flap.length.ToString("0.###");
            EditorSceneManager.OpenScene(P03SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return summary;
        }

        /// <summary>카메라 하나를 RenderTexture 로 렌더해 PNG 로 쓴다 — `capture game_view --source camera` 가 그림을 못 낼 때의 대체 경로.</summary>
        public static void RenderToPng(Camera camera, string path, int width, int height)
        {
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, DEPTH_BITS);
            descriptor.sRGB = true;
            descriptor.msaaSamples = MSAA_SAMPLES;
            RenderTexture target = RenderTexture.GetTemporary(descriptor);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(image);
        }

        private static Transform GetRoot(Scene scene)
        {
            foreach (GameObject candidate in scene.GetRootGameObjects())
            {
                if (candidate.name == P03SampleSceneBuilder.ROOT_NAME)
                {
                    return candidate.transform;
                }
            }
            Debug.Assert(false, "씬에 루트가 없다: " + P03SampleSceneBuilder.ROOT_NAME);
            return null;
        }

        private static Dictionary<string, AnimationClip> LoadClips(string modelPath)
        {
            Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
            foreach (Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(modelPath))
            {
                AnimationClip clip = asset as AnimationClip;
                if (clip != null)
                {
                    clips[clip.name] = clip;
                }
            }
            return clips;
        }
    }
}
