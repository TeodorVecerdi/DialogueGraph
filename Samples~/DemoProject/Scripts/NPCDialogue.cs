using DialogueGraph.Runtime;
using TMPro;
using UnityEngine;

public class NPCDialogue : MonoBehaviour {
    public RuntimeDialogueGraph DialogueSystem;
    public LineController LineController;

    [Header("UI References")]
    public GameObject SecondaryScreen;
    public GameObject PlayerContainer;
    public GameObject NpcContainer;
    public TMP_Text PlayerText;
    public TMP_Text NpcText;
    public TMP_Text NpcName;

    private bool metBefore = false;
    private bool isAngry = false;

    private bool isInConversation = false;
    private bool showingSecondaryScreen;
    private bool showPlayer;
    private bool isPlayerChoosing;
    private bool shouldShowText;
    private bool showingText;
    private string textToShow;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z) && !isInConversation) {
            metBefore = false;
            isAngry = false;
        }

        if (Input.GetKeyDown(KeyCode.F) && !isInConversation) {
            DialogueSystem.ResetConversation();
            isInConversation = true;
            (showPlayer ? PlayerContainer : NpcContainer).SetActive(true);
        }

        if (showingSecondaryScreen) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                showingSecondaryScreen = false;
                SecondaryScreen.SetActive(false);
            }
            return;
        }

        if (!isInConversation || isPlayerChoosing) return;
        if (shouldShowText) {
            (showPlayer ? PlayerContainer : NpcContainer).SetActive(true);
            (showPlayer ? PlayerText : NpcText).gameObject.SetActive(true);
            (showPlayer ? PlayerText : NpcText).text = textToShow;
            showingText = true;
            shouldShowText = false;
        }

        if (showingText) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                showingText = false;
                (showPlayer ? PlayerContainer : NpcContainer).SetActive(false);
                (showPlayer ? PlayerText : NpcText).gameObject.SetActive(false);
            }
        } else {
            if (DialogueSystem.IsConversationDone()) {
                // Reset state
                isInConversation = false;
                showingSecondaryScreen = false;
                showPlayer = false;
                isPlayerChoosing = false;
                shouldShowText = false;
                showingText = false;

                PlayerContainer.SetActive(false);
                NpcContainer.SetActive(false);
                return;
            }

            var isNpc = DialogueSystem.IsCurrentNpc();
            if (isNpc) {
                var currentActor = DialogueSystem.GetCurrentActor();
                showPlayer = false;
                shouldShowText = true;
                textToShow = DialogueSystem.ProgressNpc();
                NpcName.text = currentActor.Name;
            } else {
                var currentLines = DialogueSystem.GetCurrentLines();
                isPlayerChoosing = true;
                PlayerContainer.SetActive(true);
                LineController.gameObject.SetActive(true);
                LineController.Initialize(currentLines);
            }
        }
    }

    public void PlayerSelect(int index) {
        LineController.gameObject.SetActive(false);
        textToShow = DialogueSystem.ProgressSelf(index);
        isPlayerChoosing = false;
        shouldShowText = true;
        showPlayer = true;
    }

    public bool MetBefore(string node, int lineIndex) {
        return metBefore;
    }

    public bool Angry(string node, int lineIndex) {
        return isAngry;
    }

    public void Meet(string node, int lineIndex) {
        metBefore = true;
    }

    public void MakeAngry(string node, int lineIndex) {
        isAngry = true;
    }

    public void ClearAngry(string node, int lineIndex) {
        isAngry = false;
    }

    public void PlayGame(string node, int lineIndex) {
        showingSecondaryScreen = true;
        SecondaryScreen.SetActive(true);

        NpcContainer.SetActive(false);
        PlayerContainer.gameObject.SetActive(false);
        showingText = false;
        PlayerText.gameObject.SetActive(false);
        NpcText.gameObject.SetActive(false);
    }

    public void OpenShop(string node, int lineIndex) {
        showingSecondaryScreen = true;
        SecondaryScreen.SetActive(true);

        NpcContainer.SetActive(false);
        PlayerContainer.gameObject.SetActive(false);
        showingText = false;
        PlayerText.gameObject.SetActive(false);
        NpcText.gameObject.SetActive(false);
    }
}