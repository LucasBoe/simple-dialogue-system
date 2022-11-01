using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Simple.DialogueTree.Nodes
{
    [System.Serializable]
    public class ChoiceOption : ScriptableObject, ILocalizeableText
    {
        [SerializeField, ReadOnly] string guid;
        [SerializeField, ReadOnly] bool isLocalized = false;
        public bool IsLocalized { get => isLocalized; }
        public string Text;

        string ILocalizeableText.TextValue => Text;
        public SerializedProperty TextProperty { get => new SerializedObject(this).FindProperty("Text"); }

        public DialogueNode Next;
        public void GenerateGUID() => guid = UnityEditor.GUID.Generate().ToString();
        public string Guid => guid;
        public void SetLocalized(bool localized, string key = "")
        {
            isLocalized = localized;
            if (isLocalized) Text = key;
        }
        private void OnDestroy() => ILocalizableEvents.OnDestroyAction?.Invoke(guid);

        public void Create(string name)
        {
            this.name = name;

            //TODO: Generalize it so you don't have to write it over and over again
            GenerateGUID();
            ILocalizableEvents.OnCreateAction?.Invoke(guid);

            if (DialogueTreeTextProcessor.FindIsValidKey(guid))
            {
                SetLocalized(true, guid);
            }
        }
    }
}
