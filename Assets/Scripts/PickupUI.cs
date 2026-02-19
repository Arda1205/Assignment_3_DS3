using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupUI : MonoBehaviour
{
    [Header("BARS")]
    public Slider safeBar;
    public Slider moneyBar;
    public Slider valuableBar;

    [Header("TIMES")]
    public float safeFillTime = 5f;
    public float moneyFillTime = 1f;
    public float valuableFillTime = 4f;

    [Header("SAFE")]
    public GameObject safeParent;

    [Header("MONEY TOTAL UI")]
    public TextMeshProUGUI moneyText;

    [Header("TIMER")]
    public CountdownTimer countdownTimer;

    [Header("AUDIO")]
    public AudioSource audioSource;
    public AudioClip safeOpenSFX;
    public AudioClip moneyPickupSFX;
    public AudioClip valuablePickupSFX;
    //public AudioClip cancelSFX;

    private float safeValue;
    private float moneyValue;
    private float valuableValue;

    private int totalMoney = 0;

    private GameObject currentTarget;
    private string currentType = "";

    [System.Obsolete]
    void Start()
    {
        SetupBar(safeBar);
        SetupBar(moneyBar);
        SetupBar(valuableBar);

        UpdateMoneyText();

        // auto find safe
        if (safeParent == null)
        {
            GameObject safeObj = GameObject.FindWithTag("Safe");
            if (safeObj != null)
                safeParent = safeObj;
        }

        // auto find countdown timer
        if (countdownTimer == null)
        {
            countdownTimer = FindObjectOfType<CountdownTimer>();
        }
    }


    void SetupBar(Slider s)
    {
        s.minValue = 0;
        s.maxValue = 100;
        s.value = 0;
    }

    void UpdateMoneyText()
    {
        moneyText.text = "$" + totalMoney.ToString();
    }

    // Called by player when starting interaction
    public void StartInteraction(GameObject target, string type)
    {
        currentTarget = target;
        currentType = type;
    }

    // Called every frame holding E
    public void Fill(float dt)
    {
        if (currentTarget == null) return;

        switch (currentType)
        {
            case "Safe":
                safeValue += (100f / safeFillTime) * dt;
                safeBar.value = safeValue;

                if (safeValue >= 100f)
                {
                    CompleteSafe();
                }
                break;

            case "Money":
                moneyValue += (100f / moneyFillTime) * dt;
                moneyBar.value = moneyValue;

                if (moneyValue >= 100f)
                {
                    CompleteMoney();
                }
                break;

            case "Valuable":
                valuableValue += (100f / valuableFillTime) * dt;
                valuableBar.value = valuableValue;

                if (valuableValue >= 100f)
                {
                    CompleteValuable();
                }
                break;
        }
    }

    public void Cancel()
    {
        if (currentTarget == null) return;

        //if (cancelSFX != null) audioSource.PlayOneShot(cancelSFX);

        ResetBars();
        currentTarget = null;
        currentType = "";
    }

    void ResetBars()
    {
        safeValue = 0;
        moneyValue = 0;
        valuableValue = 0;

        safeBar.value = 0;
        moneyBar.value = 0;
        valuableBar.value = 0;
    }

    void CompleteSafe()
    {
        if (safeOpenSFX) audioSource.PlayOneShot(safeOpenSFX);

        if (safeParent) safeParent.SetActive(false);

        // START COUNTDOWN WHEN SAFE OPENS
        if (countdownTimer != null)
            countdownTimer.StartCountdown();

        ResetBars();
        currentTarget = null;
    }


    void CompleteMoney()
    {
        if (moneyPickupSFX) audioSource.PlayOneShot(moneyPickupSFX);

        totalMoney += 1000;
        UpdateMoneyText();

        currentTarget.SetActive(false);

        ResetBars();
        currentTarget = null;
    }

    void CompleteValuable()
    {
        if (valuablePickupSFX) audioSource.PlayOneShot(valuablePickupSFX);

        totalMoney += 8000;
        UpdateMoneyText();

        currentTarget.SetActive(false);

        ResetBars();
        currentTarget = null;
    }
}
