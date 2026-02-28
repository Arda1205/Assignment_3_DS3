using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;

// Manages interaction progress bars and visual feedback for item and safe interactions
public class PickupUI : MonoBehaviour
{
    [Header("BARS")]
    public Slider safeBar;
    public Slider moneyBar;
    public Slider valuableBar;

    [Header("TIMES")]
    public float safeFillTime = 5f; // kept for local smoothing if needed
    public float moneyFillTime = 1f;
    public float valuableFillTime = 3f;

    [Header("SAFE")]
    public GameObject safeParent; // optional, scene safe

    [Header("MONEY TOTAL UI")]
    public TextMeshProUGUI moneyText;

    [Header("TIMER")]
    public CountdownTimer countdownTimer;

    [Header("AUDIO")]
    public AudioSource audioSource;
    public AudioClip safeOpenSFX;
    public AudioClip moneyPickupSFX;
    public AudioClip valuablePickupSFX;

    private float safeValueLocal;
    private float moneyValue;
    private float valuableValue;

    private GameObject currentTarget;
    private string currentType = "";

    private PlayerState myState;

    [Obsolete]
    void Start()
    {
        // Init bars to 0..100
        SetupBar(safeBar);
        SetupBar(moneyBar);
        SetupBar(valuableBar);

        // Find PlayerState on same prefab (this canvas is on the player prefab)
        myState = GetComponentInParent<PlayerState>();
        if (myState != null)
        {
            UpdateMoneyText(myState.Money.Value);
            myState.Money.OnValueChanged += OnMoneyChanged;
        }
        else
        {
            UpdateMoneyText(0);
        }

        // Auto-find safe if not assigned
        if (safeParent == null)
        {
            var found = GameObject.FindWithTag("Safe");
            if (found != null)
                safeParent = found;
        }

        if (safeParent != null)
        {
            var safeNet = safeParent.GetComponent<SafeNetwork>();
            if (safeNet != null)
            {
                // Initial set
                safeBar.value = safeNet.Progress.Value * 100f;

                // Aubscribe
                safeNet.Progress.OnValueChanged += (oldv, newv) => { safeBar.value = newv * 100f; };

                safeNet.IsOpen.OnValueChanged += (oldv, newv) => {
                    if (newv) safeBar.value = 100f;
                };
            }
        }


        // Auto find countdown timer if null
        if (countdownTimer == null)
        {
            countdownTimer = FindObjectOfType<CountdownTimer>();
        }
    }

    void OnDestroy()
    {
        if (myState != null)
            myState.Money.OnValueChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(int oldVal, int newVal)
    {
        UpdateMoneyText(newVal);
    }

    void SetupBar(Slider s)
    {
        s.minValue = 0;
        s.maxValue = 100;
        s.value = 0;
    }

    void UpdateMoneyText(int value)
    {
        if (moneyText != null)
            moneyText.text = "$" + value.ToString();
    }

    // Called by PlayerInteraction when starting interaction (local only)
    public void StartInteraction(GameObject target, string type)
    {
        currentTarget = target;
        currentType = type;
    }

    // Called every frame while E held (local UI fill)
    public void Fill(float dt)
    {
        if (currentTarget == null) return;

        switch (currentType)
        {
            case "Safe":
                // safe progress is authoritative on server; we still show local increment while player holds
                // local bar will be overwritten by SafeNetwork.Progress OnValueChanged subscription
                break;

            case "Money":
                moneyValue += (100f / moneyFillTime) * dt;
                moneyBar.value = moneyValue;
                if (moneyValue >= 100f)
                {
                    // request server to pickup this item
                    var pickupNet = currentTarget.GetComponent<PickupNetwork>();
                    if (pickupNet != null)
                        pickupNet.RequestPickupServerRpc();

                    if (moneyPickupSFX != null) audioSource.PlayOneShot(moneyPickupSFX);

                    ResetBars();
                    currentTarget = null;
                }
                break;

            case "Valuable":
                valuableValue += (100f / valuableFillTime) * dt;
                valuableBar.value = valuableValue;
                if (valuableValue >= 100f)
                {
                    var pickupNet = currentTarget.GetComponent<PickupNetwork>();
                    if (pickupNet != null)
                        pickupNet.RequestPickupServerRpc();

                    if (valuablePickupSFX != null) audioSource.PlayOneShot(valuablePickupSFX);

                    ResetBars();
                    currentTarget = null;
                }
                break;
        }
    }

    public void Cancel()
    {
        ResetBars();
        currentTarget = null;
        currentType = "";
    }

    void ResetBars()
    {
        moneyValue = 0f;
        valuableValue = 0f;
        moneyBar.value = 0f;
        valuableBar.value = 0f;
    }
}
