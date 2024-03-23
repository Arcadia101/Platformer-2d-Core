using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance{get; private set; }

    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCams;
    private CinemachineVirtualCamera _currentCam;
    private CinemachineFramingTransposer framingTransposer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        for (int i = 0; i < _allVirtualCams.Length; i++)
        {
            if (_allVirtualCams[i].enabled)
            {
                _currentCam = _allVirtualCams[i];
                framingTransposer = _currentCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }
    }

    private void Start()
    {
        for (int i = 0; i < _allVirtualCams.Length; i++)
        {
            _allVirtualCams[i].Follow = CharacterStats.Instance.transform;
        }
    }

    public void SwapCamera(CinemachineVirtualCamera newCam)
    {
        _currentCam.enabled = false;

        _currentCam = newCam;

        _currentCam.enabled = true;
    }
}
