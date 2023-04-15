using UnityEngine;
using UnityEditor;
using Mediapipe.Unity.FaceMesh;
using System.Collections.Generic;
using Mediapipe;

public class GlassesTrack : MonoBehaviour
{
  public float smoothTime = 0.001f;
  public int glassesScaleFactor = 25;
  public float distanceFactor = 50f;
  public float rotationSpeed = 1f;

  private Vector3 _leftEyePos;
  private Vector3 _rightEyePos;
  private Vector3 _noseTipPos;
  private Vector3 _velocity = Vector3.zero;

  private void Update()
  {
    var interfaceWidth = 1000f; // width of your interface rectangle in pixels
    var unityScreenWidth = 10f; // width of your Unity screen in units
    var scaleFactor = unityScreenWidth / interfaceWidth;
    Debug.Log(scaleFactor);

    // position
    var eyeVector = _rightEyePos - _leftEyePos;
    var eyeMidpoint = (_leftEyePos + _rightEyePos) / 2f;
    var noseVector = _noseTipPos - eyeMidpoint;
    var projection = Vector3.Project(noseVector, eyeVector);
    var targetPos = eyeMidpoint + (projection * distanceFactor);
    targetPos.z = -100f;
    var originalZ = transform.position.z;


    transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
    transform.position = new Vector3(targetPos.x * scaleFactor, targetPos.y * scaleFactor, originalZ);
    Debug.Log("transform1" + transform.position);




    // Get rotation for X axis (using eyeMidpoint - noseTipPos vector)
    var lookDirX = Quaternion.Inverse(transform.rotation) * (eyeMidpoint - _noseTipPos);
    lookDirX = new Vector3(-lookDirX.x, -lookDirX.y, -lookDirX.z);
    var targetRotX = Quaternion.LookRotation(lookDirX, Vector3.up);
    //targetRotX *= Quaternion.Euler(-40f, 0f, 0f);

    // Get rotation for Y axis (using leftEye - rightEye vector)
    //var lookDirY = Quaternion.Inverse(transform.rotation) * eyeVector;
    //lookDirY = new Vector3(lookDirY.x, lookDirY.y, -lookDirY.z - 10f);
    //var targetRotY = Quaternion.LookRotation(lookDirY, Vector3.up);

    // Combine rotations and set rotation of transform
    var targetRot = targetRotX;
    //var targetRot = targetRotY;
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothTime);

    //scale
    var eyeDist = Vector3.Distance(_leftEyePos, _rightEyePos);
    var scale = glassesScaleFactor * eyeDist;
    transform.localScale = new Vector3(scale, scale, scale);
  }

  public void UpdateGlassesPosition(List<NormalizedLandmarkList> landmarkList)
  {
    if (landmarkList == null) return;

    var leftEyePos = landmarkList[0].Landmark[33];
    var rightEyePos = landmarkList[0].Landmark[263];

    //foreach (var item in landmarkList)
    //{
    //  Debug.Log("LandmarkItems: " + item);
    //}
    var nosePos = landmarkList[0].Landmark[2];

    // Invert x and y to match Unity's coordinate system
    _leftEyePos = new Vector3(-leftEyePos.X, -leftEyePos.Y, -leftEyePos.Z);
    _rightEyePos = new Vector3(-rightEyePos.X, -rightEyePos.Y, -rightEyePos.Z);
    _noseTipPos = new Vector3(-nosePos.X, -nosePos.Y, -nosePos.Z);
  }
}
