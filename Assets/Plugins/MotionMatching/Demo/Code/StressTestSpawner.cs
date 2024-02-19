using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MxM;

public class StressTestSpawner : MonoBehaviour
{
    public GameObject SpawnPrefab;
    public int Rows = 10;
    public int Cols = 10;

    public float Spacing = 1f;
    public bool OffsetUpdates = false;

    public Slider UpdateIntervalSlider;
    public Text UpdateIntervalValue;
    public InputField ColumnsInputField;
    public InputField RowsInputField;
    public Text FPS;
    public Text AveFPS;

    private float cumulativeFPS = 0f;

    private List<MxMAnimator> m_actors;
    private List<float> m_fpsFrames;

    private void Start()
    {
        m_actors = new List<MxMAnimator>(101);
        m_fpsFrames = new List<float>(61); 

        ColumnsInputField.text = Rows.ToString();
        RowsInputField.text = Cols.ToString();
    }

    public void Spawn()
    {
        ClearActors();

        if(SpawnPrefab != null)
        {
            if (OffsetUpdates)
            {
                StartCoroutine(OffsetSpawn());
            }
            else
            {
                SpawnIdentical();
            }
        }
    }

    public void ClearActors()
    {
        foreach(MxMAnimator actor in m_actors)
        {
            Destroy(actor.gameObject);
        }

        m_actors.Clear();
    }

    public void Update()
    {
        float fps = 1f / Time.deltaTime;

        FPS.text = fps.ToString("F0");

        if(m_fpsFrames.Count == 60)
        {
            cumulativeFPS -= m_fpsFrames[0];
            m_fpsFrames.RemoveAt(0);
            
        }

        cumulativeFPS += fps;
        m_fpsFrames.Add(fps);

        AveFPS.text = (cumulativeFPS / m_fpsFrames.Count).ToString("F0");
    }

    private void SpawnIdentical()
    {
        Vector3 basePosition = transform.position;

        for (int row = 0; row < Rows; ++row)
        {
            for (int col = 0; col < Cols; ++col)
            {
                GameObject newSpawn = Instantiate(SpawnPrefab);
                newSpawn.transform.position = new Vector3(12.3f, 0f, 17f) - new Vector3(Rows * Spacing / 2f, 0f, Cols * Spacing / 2f) + new Vector3(row * Spacing, 0f, col * Spacing);
                newSpawn.SetActive(true);

                MxMAnimator animator = newSpawn.GetComponent<MxMAnimator>();
                animator.UpdateInterval = UpdateIntervalSlider.value;
                m_actors.Add(animator);
            }
        }
    }

    private IEnumerator OffsetSpawn()
    {
        Vector3 basePosition = transform.position;

        for (int row = 0; row < Rows; ++row)
        {
            for (int col = 0; col < Cols; ++col)
            {
                GameObject newSpawn = Instantiate(SpawnPrefab);
                newSpawn.transform.position = new Vector3(12.3f, 0f, 17f) - new Vector3(Rows * Spacing / 2f, 0f, Cols * Spacing / 2f) + new Vector3(row * Spacing, 0f, col * Spacing);
                newSpawn.SetActive(true);

                MxMAnimator animator = newSpawn.GetComponent<MxMAnimator>();
                animator.UpdateInterval = UpdateIntervalSlider.value;
                m_actors.Add(animator);

                yield return null;
            }
        }
    }

    public void SetColums()
    {
        Cols = int.Parse(ColumnsInputField.text);
    }

    public void SetRows()
    {
        Rows = int.Parse(RowsInputField.text);
    }

    public void SetActorUpdateInterval()
    {
        if (UpdateIntervalSlider != null && m_actors != null)
        {
            foreach (MxMAnimator mxmAnimator in m_actors)
            {
                mxmAnimator.UpdateInterval = UpdateIntervalSlider.value;
            }
        }
    }

    public void SetUpdateIntervalValue()
    {
        UpdateIntervalValue.text = UpdateIntervalSlider.value.ToString("F3");
    }
}