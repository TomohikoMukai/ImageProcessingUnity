using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageProcessing : MonoBehaviour
{
    struct ThreadSize
    {
        public uint x;
        public uint y;
        public uint z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private Texture2D _tex;
    private RenderTexture _result;

    private void Start()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Comppute Shader is not support.");
            return;
        }

        // RenderTextueの初期化
        _result = new RenderTexture(_tex.width, _tex.height, 0, RenderTextureFormat.ARGB32);
        _result.enableRandomWrite = true;
        _result.Create();

        // テクスチャを適用する
        GetComponent<MeshRenderer>().material.mainTexture = _result;
    }
    void Update()
    {
        // ComputeShaderのカーネルインデックス(0)を取得
        var kernelIndex = _computeShader.FindKernel("Process");
        // 一つのグループの中に何個のスレッドがあるか
        ThreadSize threadSize = new ThreadSize();
        _computeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSize.x, out threadSize.y, out threadSize.z);

        // GPUにデータをコピーする
        _computeShader.SetTexture(kernelIndex, "Texture", _tex);
        _computeShader.SetTexture(kernelIndex, "Result", _result);
        _computeShader.SetFloat("time", Time.time);

        // GPUの処理を実行する
        _computeShader.Dispatch(kernelIndex, _tex.width / (int)threadSize.x, _tex.height / (int)threadSize.y, (int)threadSize.z);
    }

    private void OnDestroy()
    {
        _result = null;
    }
}
