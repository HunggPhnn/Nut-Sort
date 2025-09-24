using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public class LoadingBar : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider.value = 0; // bắt đầu từ 0
        StartCoroutine(FakeLoading());
    }

    IEnumerator FakeLoading()
    {
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.4f; // tốc độ chạy
            slider.value = progress;

            yield return null; // cập nhật mỗi frame
        }
        SceneManager.LoadScene("MainMenu"); 
    }
}
