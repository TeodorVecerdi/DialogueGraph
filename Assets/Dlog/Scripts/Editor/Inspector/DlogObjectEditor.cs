using System.Linq;
using Dlog;
using Dlog.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using SerializedProperty = UnityEditor.SerializedProperty;

[CustomEditor(typeof(DialogueGraph))]
public class DlogObjectEditor : Editor {
    private DialogueGraph dialogueGraph;
    private SerializedProperty dlogObjectProperty;
    private VisualElement rootElement;
    private DlogObject DlogObject {
        get => (DlogObject) dlogObjectProperty.objectReferenceValue;
        set => dlogObjectProperty.objectReferenceValue = value;
    }
    
    public void OnEnable() {
        dialogueGraph = (DialogueGraph) target;
        rootElement = new VisualElement();
        dlogObjectProperty = serializedObject.FindProperty("DlogObject");

        var visualTree = Resources.Load<VisualTreeAsset>("DlogObjectEditor");
        visualTree.CloneTree(rootElement);
        rootElement.AddStyleSheet("DlogObjectEditor");
    }

    public override VisualElement CreateInspectorGUI() {
        var dlogObjectField = rootElement.Q<ObjectField>("dlogObjectField");
        var propertiesContainer = rootElement.Q<VisualElement>("propertiesContainer");
        var invalidContainer = rootElement.Q<VisualElement>("invalidContainer");
        
        RefreshDlogObjectView(propertiesContainer, invalidContainer);
        dlogObjectField.objectType = typeof(DlogObject);
        dlogObjectField.BindProperty(dlogObjectProperty);
        dlogObjectField.value = dlogObjectProperty.objectReferenceValue;
        dlogObjectField.RegisterCallback<ChangeEvent<Object>>(evt => {
            dlogObjectProperty.objectReferenceValue = (DlogObject) evt.newValue;
            RefreshDlogObjectView(propertiesContainer, invalidContainer);
            serializedObject.ApplyModifiedProperties();
            UpdateInspectorProperties();
        });
        UpdateInspectorProperties();
        return rootElement;
    }

    private void RefreshDlogObjectView(VisualElement propertiesContainer, VisualElement invalidContainer) {
        if (dlogObjectProperty.objectReferenceValue == null) {
            propertiesContainer.AddToClassList("hidden");
            invalidContainer.RemoveFromClassList("hidden");
        } else {
            propertiesContainer.RemoveFromClassList("hidden");
            invalidContainer.AddToClassList("hidden");
        }
    }

    private void UpdateInspectorProperties() {
        var actorContainer = rootElement.Q("actorList");
        actorContainer.Clear();
        foreach (var actorProperty in DlogObject.Properties.Where(property => property.Type == PropertyType.Actor)) {
            
        }
        
        var checkContainer = rootElement.Q("checkList");
        checkContainer.Clear();
        foreach (var checkProperty in DlogObject.Properties.Where(property => property.Type == PropertyType.Check)) {
            
        }
        
        var triggerContainer = rootElement.Q("triggerList");
        triggerContainer.Clear();
        foreach (var triggerProperty in DlogObject.Properties.Where(property => property.Type == PropertyType.Trigger)) {
            
        }
    }
}