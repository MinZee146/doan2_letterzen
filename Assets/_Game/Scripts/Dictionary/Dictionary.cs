using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Dictionary : SingletonPersistent<Dictionary>
{
    private HashSet<string> _words = new();
    private TextAsset _dictText;

    public void Initialize()
    {
        Addressables.LoadAssetAsync<TextAsset>(Constants.DICTIONARY_KEY).Completed += OnDictionaryLoaded;
    }

    private void OnDictionaryLoaded(AsyncOperationHandle<TextAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _dictText = handle.Result;

            using var reader = new StringReader(_dictText.text);
            while (reader.ReadLine() is { } line)
            {
                _words.Add(line);
            }

            Debug.Log("Dictionary loaded and processed.");
        }
        else
        {
            Debug.LogError("Failed to load dictionary asset from Addressables.");
        }
    }

    public bool CheckWord(string word)
    {
        return word != null && _words.Contains(word.ToLower());
    }

    public void UnloadDictionary()
    {
        _words.Clear();
        Addressables.Release(_dictText);
    }
}