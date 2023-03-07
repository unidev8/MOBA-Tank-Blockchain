/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

namespace TanksMP
{
  /// <summary>
  /// Camera script for following the player or a different target transform.
  /// Extended with ability to hide certain layers (e.g. UI) while in "follow mode".
  /// </summary>
  public class FollowTarget : MonoBehaviour
  {
    /// <summary>
    /// The camera target to follow.
    /// Automatically picked up in LateUpdate().
    /// </summary>
    public Transform target;

    /// <summary>
    /// Layers to hide after calling HideMask().
    /// </summary>
    public LayerMask respawnMask;

    /// <summary>
    /// The clamped distance in the x-z plane to the target.
    /// </summary>
    public float distance = 10.0f;

    /// <summary>
    /// The clamped height the camera should be above the target.
    /// </summary>
    public float height = 5.0f;
    public float difference = 5.0f;
    public float roateChange = 0.0f;
    public float moveSpeedx = 3f;
    public float moveSpeedy = -1f;
    bool rightclicked = false;

    /// <summary>
    /// Reference to the Camera component.
    /// </summary>
    [HideInInspector]
    public Camera cam;

    /// <summary>
    /// Reference to the camera Transform.
    /// </summary>
    [HideInInspector]
    public Transform camTransform;

    //initialize variables
    void Start()
    {
      UIManager.instance.ShowLoading(false);
      cam = GetComponent<Camera>();
      camTransform = transform;
      roateChange = transform.eulerAngles.y;
      //the AudioListener for this scene is not attached directly to this camera,
      //but to a separate gameobject parented to the camera. This is because the
      //camera is usually positioned above the player, however the AudioListener
      //should consider audio clips from the position of the player in 3D space.
      //so here we position the AudioListener child object at the target position.
      //Remark: parenting the AudioListener to the player doesn't work, because
      //it gets disabled on death and therefore stops playing sounds completely
      Transform listener = GetComponentInChildren<AudioListener>().transform;
      listener.position = transform.position + transform.forward * distance;
    }

    //position the camera in every frame

    void HeightScroll()
    {
      float tempDistance = distance + -1 * Input.mouseScrollDelta.y * Time.deltaTime * 60f;
      if (tempDistance >= 1 && tempDistance <= 20)
      {
        distance += -1 * Input.mouseScrollDelta.y * Time.deltaTime * 60f;
      }
    }
    void RotateCamera()
    {
      
      if (Input.GetMouseButton(1))
      {
        roateChange += Input.GetAxis("Mouse X") * moveSpeedx;

        float tempHeight = height + Input.GetAxis("Mouse Y") * moveSpeedy;
        if (tempHeight >= 2 && tempHeight <= 30)
        {
          height = tempHeight;
        }
      }
    }

    void LateUpdate()
    {
      HeightScroll();
      RotateCamera();
      //cancel if we don't have a target
      if (!target)
        return;

      //convert the camera's transform angle into a rotation
      Quaternion currentRotation = Quaternion.Euler(0, roateChange, 0);

      //set the position of the camera on the x-z plane to:
      //distance units behind the target, height units above the target
      Vector3 pos = target.position;
      pos -= currentRotation * Vector3.forward * Mathf.Abs(distance);
      pos.y = target.position.y + Mathf.Abs(height);
      transform.position = pos;

      //look at the target
      transform.LookAt(target);

      //clamp distance
      transform.position = target.position - (transform.forward * Mathf.Abs(distance));
      transform.position = transform.position + new Vector3(0f, 0f, difference);
    }


    /// <summary>
    /// Culls the specified layers of 'respawnMask' by the camera.
    /// </summary>
    public void HideMask(bool shouldHide)
    {
      if (shouldHide) cam.cullingMask &= ~respawnMask;
      else cam.cullingMask |= respawnMask;
    }
  }
}