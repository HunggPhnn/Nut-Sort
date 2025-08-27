using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("References")]
    public Transform ringHoldPoint; // đặt child empty này trong prefab/scene

    [Header("Settings")]
    public int maxRings = 4;
    public float ringSpacing = 0.4f; // khoảng cách giữa các vòng

    // runtime list
    [HideInInspector] public List<Transform> rings = new List<Transform>();

    private void Start()
    {
        UpdateRings();
    }

    // Đọc các child hiện tại của ringHoldPoint vào danh sách rings
    public void UpdateRings()
    {
        rings.Clear();
        if (ringHoldPoint == null) return;
        foreach (Transform child in ringHoldPoint)
        {
            rings.Add(child);
        }
        RepositionRings();
    }

    // Trả về ring trên cùng (top)
    public Transform GetTopRing()
    {
        if (rings.Count == 0) return null;
        return rings[rings.Count - 1];
    }

    // Kiểm tra có còn chỗ để đặt ring
    public bool CanPlaceRing()
    {
        return rings.Count < maxRings;
    }

    public bool CanPlaceRing(Transform ring)
    {
        if (!CanPlaceRing()) return false;
        if (rings.Count == 0) return true;

        Renderer topR = rings[rings.Count - 1].GetComponent<Renderer>();
        Renderer movingR = ring.GetComponent<Renderer>();
        if (topR == null || movingR == null) return true; 

        return topR.sharedMaterial == movingR.sharedMaterial;
    }
    
    // Thêm ring (sẽ set parent và đặt vị trí đúng)
    public void AddRing(Transform ring)
{
    if (ring == null || ringHoldPoint == null) return;

    int index = rings.Count;

    ring.SetParent(ringHoldPoint);
    float baseOffset = -0.8f; 

    ring.localPosition = new Vector3(0f, baseOffset + index * ringSpacing, 0f);
    rings.Add(ring);
}


    // Remove ring khỏi danh sách (không đặt parent)
    public void RemoveRing(Transform ring)
    {
        if (ring == null) return;
        rings.Remove(ring);
        // không setParent ở đây, caller sẽ tự detach khi cần
        RepositionRings();
    }

    // Cập nhật tất cả vị trí vòng theo danh sách rings
    public void RepositionRings()
    {
        for (int i = 0; i < rings.Count; i++)
        {
            if (rings[i] == null) continue;
            rings[i].SetParent(ringHoldPoint);
        }
    }
    // Kiểm tra tower đã thắng chưa
    public bool IsTowerComplete(){
        if (rings.Count < maxRings) return false;

        // Lấy màu vòng đầu tiên
        Renderer firstR = rings[0].GetComponent<Renderer>();
        if (firstR == null) return false;
        Material targetMat = firstR.sharedMaterial;

        // So sánh tất cả các vòng còn lại
        for (int i = 1; i < rings.Count; i++){
            Renderer r = rings[i].GetComponent<Renderer>();
            if (r == null || r.sharedMaterial != targetMat)
            return false;
        }
        return true;
    }
}
