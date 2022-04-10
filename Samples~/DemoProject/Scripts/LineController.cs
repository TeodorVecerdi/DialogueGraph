using System;
using System.Collections;
using System.Collections.Generic;
using DialogueGraph.Runtime;
using UnityEngine;

public class LineController : MonoBehaviour {
    public LineEntry Prefab;
    public NPCDialogue Owner;
    private List<LineEntry> entries;
    private int selectedIndex;
    private bool isActive;

    public void Clear() {
        entries.ForEach(entry => Destroy(entry.gameObject));
        entries.Clear();
    }

    public void Initialize(List<ConversationLine> lines) {
        isActive = true;
        entries = new List<LineEntry>();
        for (var i = 0; i < lines.Count; i++) {
            var line = lines[i];
            var entry = Instantiate(Prefab, transform);
            entry.Initialize(line.Message);
            entries.Add(entry);
        }

        selectedIndex = 0;
        entries[0].Select(true);
    }

    public void SelectLine(int index) {
        Clear();
        isActive = false;
        selectedIndex = -1;
        Owner.PlayerSelect(index);
    }

    private void Update() {
        if(!isActive) return;
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            var nextIndex = Mathf.Min(selectedIndex + 1, entries.Count - 1);
            entries[selectedIndex].Select(false);
            entries[nextIndex].Select(true);
            selectedIndex = nextIndex;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            var nextIndex = Mathf.Max(selectedIndex - 1, 0);
            entries[selectedIndex].Select(false);
            entries[nextIndex].Select(true);
            selectedIndex = nextIndex;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            SelectLine(selectedIndex);
        }
    }
}
