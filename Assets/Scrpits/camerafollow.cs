using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform ratTransform; // Drag your Rat/Bot here

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 5, -5); // Position relative to the rat
    public float smoothSpeed = 0.125f; // Lower is smoother, higher is snappier

    void LateUpdate()
    {
        if (ratTransform == null) return;

        // 1. Calculate the desired position
        Vector3 desiredPosition = ratTransform.position + offset;

        // 2. Smoothly move from current position to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 3. Apply the position
        transform.position = smoothedPosition;

        // 4. Always look at the rat
        transform.LookAt(ratTransform);
    }

    // Call this when the maze resets so the camera snaps instantly
    public void SnapToTarget()
    {
        if (ratTransform != null)
        {
            transform.position = ratTransform.position + offset;
        }
    }
}