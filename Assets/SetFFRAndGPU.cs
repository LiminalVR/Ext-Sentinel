using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFFRAndGPU : MonoBehaviour
{
    public OVRManager.FixedFoveatedRenderingLevel FFRLevel;
    public bool SetGPULevel;
    public int TargetGPULevel;

    public bool SetCPULevel; // New field for CPU level
    public int TargetCPULevel; // New target CPU level

    private int _cachedGPULevel;
    private int _cachedCPULevel; // Cached CPU level

    // Start is called before the first frame update
    public void Awake()
    {
        _cachedGPULevel = OVRManager.gpuLevel;
        _cachedCPULevel = OVRManager.cpuLevel; // Cache current CPU level

        SetFFR();
        SetGPU();
        SetCPU(); // Set CPU level
    }

    void OnDestroy()
    {
        ResetFFR();
        ResetGPU();
        ResetCPU(); // Reset CPU level
    }

    public void SetFFR()
    {
        OVRManager.fixedFoveatedRenderingLevel = FFRLevel;
    }

    public void SetGPU()
    {
        if (SetGPULevel)
            OVRManager.gpuLevel = TargetGPULevel;
    }

    public void ResetGPU()
    {
        if (OVRManager.gpuLevel != _cachedGPULevel)
            OVRManager.gpuLevel = _cachedGPULevel;
    }

    public void SetCPU() // Function to set the CPU level
    {
        if (SetCPULevel)
            OVRManager.cpuLevel = TargetCPULevel;
    }

    public void ResetCPU() // Function to reset the CPU level
    {
        if (OVRManager.cpuLevel != _cachedCPULevel)
            OVRManager.cpuLevel = _cachedCPULevel;
    }

    public void ResetFFR()
    {
        OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Off;
    }
}
