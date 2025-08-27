using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RingDrag : MonoBehaviour
{
    private Camera cam;
    private Vector3 originalPosition;
    private Tower originalTower;
    private bool isDragging = false;

    [Header("Drag Settings")]
    public float snapDistance = 3.0f;    // tối đa khoảng cách từ ring tới ringHoldPoint để snap
    
    [Header("Âm thanh")]
    public AudioClip placeRingSound;
    Collider myCollider;

    void Start()
    {
        cam = Camera.main;
        myCollider = GetComponent<Collider>();
    }

    void OnMouseDown()
    {
        // Lưu vị trí ban đầu
        originalPosition = transform.position;

        // Tìm Tower gốc (nếu ring là child của ringHoldPoint)
        originalTower = GetComponentInParent<Tower>();

        // Chỉ cho kéo nếu ring là top ring của tower gốc
        if (originalTower == null)
        {
            return;
        }

        if (originalTower.GetTopRing() != transform)
        {
            // không phải vòng trên cùng -> không cho kéo
            return;
        }

        // Remove khỏi tower (cập nhật danh sách)
        originalTower.RemoveRing(transform);

        // Detach để giữ vị trí world
        transform.SetParent(null);

        // Disable collider khi kéo để tránh chặn raycast (tùy trường hợp)
        if (myCollider != null) myCollider.enabled = false;

        isDragging = true;
    }

    void OnMouseDrag(){
        if (!isDragging) return;

        // Tạo mặt phẳng song song với camera, đi qua vị trí hiện tại của ring
        Plane plane = new Plane(cam.transform.forward * -1, transform.position);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter)){
        Vector3 hitPoint = ray.GetPoint(enter);
        transform.position = hitPoint; 
        }
    }

    void OnMouseUp()
    {
        if (!isDragging)
        {
            // đảm bảo re-enable collider nếu bị tắt trước đó
            if (myCollider != null && !myCollider.enabled) myCollider.enabled = true;
            return;
        }

        isDragging = false;

        // Re-enable collider
        if (myCollider != null) myCollider.enabled = true;

        // Tìm trụ gần nhất (bằng khoảng cách tới ringHoldPoint)
        Tower[] all = FindObjectsOfType<Tower>();
        Tower nearest = null;
        float minDist = float.MaxValue;
        Vector3 pos = transform.position;
        foreach (Tower t in all)
        {
            if (t == null || t.ringHoldPoint == null) continue;
            float d = Vector3.Distance(t.ringHoldPoint.position, pos);
            if (d < minDist)
            {
                minDist = d;
                nearest = t;
            }
        }
        // Nếu tìm được tower trong snapDistance và thỏa luật -> add vào tower đó
        if (nearest != null && minDist <= snapDistance)
        {
            bool canPlace = nearest.CanPlaceRing(); 
            if (canPlace)
            {
                nearest.AddRing(transform);

                // 🔊 phát âm thanh khi đặt vòng
                if (placeRingSound != null && GameManager.instance.audioSource != null)
                {
                    GameManager.instance.audioSource.PlayOneShot(placeRingSound);
                }

                GameManager.instance.CheckWinCondition();
                return;
            }
        }

        // Nếu không hợp lệ -> trả về trụ gốc (nếu có) hoặc trả về vị trí cũ
        if (originalTower != null)
        {
            originalTower.AddRing(transform);
        }
        else
        {
            transform.position = originalPosition;
        }
    }
}

