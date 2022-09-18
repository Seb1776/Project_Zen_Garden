using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{
    public Animator menuAnim;
    public Animator transitionBall;
    public AudioSource music;
    public AudioMixer masterMxr;
    public AudioClip introLaugh;
    public SerializableData fileA = null, fileB = null, fileC = null;
    public InputField gardenNameInput;
    public string newSetGardenName;
    public string loadedLetter;
    public Slider musSlider, sfxSlider;
    public float musVolume, sfxVolume;
    public FileUI fileAUI, fileBUI, fileCUI;
    public GameObject[] hands;

    private TouchScreenKeyboard keyboard;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(transitionBall.gameObject);
        LoadFiles();
        LoadSettings();
    }

    void LoadFiles()
    {
        string filePaths = Application.persistentDataPath;

        if (File.Exists(filePaths + "/ZenGardenVR_A.json"))
        {
            if (GetFileData(filePaths + "/ZenGardenVR_A.json") != null)
                fileA = GetFileData(filePaths + "/ZenGardenVR_A.json");
        }

        if (File.Exists(filePaths + "/ZenGardenVR_B.json"))
        {
            if (GetFileData(filePaths + "/ZenGardenVR_B.json") != null)
                fileB = GetFileData(filePaths + "/ZenGardenVR_B.json");
        }

        if (File.Exists(filePaths + "/ZenGardenVR_C.json"))
        {
            if (GetFileData(filePaths + "/ZenGardenVR_C.json") != null)
                fileC = GetFileData(filePaths + "/ZenGardenVR_C.json");
        }

        fileAUI.SetFileUI(fileA);
        fileBUI.SetFileUI(fileB);
        fileCUI.SetFileUI(fileC);
    }

    SerializableData GetFileData (string path)
    {
        SerializableData fileData = null;

        using (StreamReader reader = new StreamReader(path))
        {
            string json = reader.ReadToEnd();
            fileData = JsonUtility.FromJson<SerializableData>(json);
        }

        return fileData;
    }

    public void LoadGame(string gameFile)
    {
        loadedLetter = gameFile;
        StartCoroutine(PreLoadGame());
    }

    public void SetMusic(float volume)
    {
        masterMxr.SetFloat("MusVolume", volume / 2f);
        musVolume = volume / 2f;
    }

    public void SetSFX(float volume)
    {
        masterMxr.SetFloat("SFXVolume", volume / 2f);
        sfxVolume = volume / 2f;
    }

    IEnumerator AutoSaveSettings()
    {
        yield return new WaitForSeconds(.15f);
        SaveSettings();
    }

    public void SaveSettings()
    {
        GameSettings settingsData = new GameSettings(musVolume, sfxVolume);

        using (StreamWriter stream = new StreamWriter(Application.persistentDataPath +  "/ZGSettings.json"))
        {
            string json = JsonUtility.ToJson(settingsData);
            stream.Write(json);
        }
    }

    public void LoadSettings()
    {
        if (File.Exists(Application.persistentDataPath + "/ZGSettings.json"))
        {
            using (StreamReader reader = new StreamReader(Application.persistentDataPath + "/ZGSettings.json"))
            {
                string json = reader.ReadToEnd();
                GameSettings settings = JsonUtility.FromJson<GameSettings>(json);

                SetMusic(settings.musVolume); musVolume = 
                   musSlider.value = settings.musVolume;

                SetSFX(settings.sfxVolume); sfxVolume = 
                    sfxSlider.value = settings.sfxVolume;
            }
        }

        StartCoroutine(AutoSaveSettings());
    }

    public void SetNewLoadedLetter(string letter)
    {
        loadedLetter = letter;
    }

    public void LoadNewGame()
    {   
        if (gardenNameInput.text.Length >= 1)
        {
            newSetGardenName = gardenNameInput.text;
            StartCoroutine(PreLoadGame());
        }
    }

    public void DeleteSaveFile(string letter)
    {
        File.Delete(Application.persistentDataPath + "/ZenGardenVR_" + letter + ".json");

        switch (letter)
        {
            case "A": 
                fileA = null;
                fileAUI.SetFileUI(fileA);
            break;

            case "B": 
                fileB = null;
                fileBUI.SetFileUI(fileB);
            break;

            case "C": 
                fileC = null;
                fileCUI.SetFileUI(fileC);
            break;
        }
    }

    public void OpenKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    public void CloseKeyboard()
    {
        keyboard = null;
    }

    IEnumerator PreLoadGame()
    {
        foreach (GameObject g in hands) g.SetActive(false);
        transitionBall.SetTrigger("transition");
        music.Stop();
        music.PlayOneShot(introLaugh);
        yield return new WaitForSeconds(introLaugh.length);
        StartCoroutine(LoadScene(1));
    }

    IEnumerator LoadScene(int index)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(index);

        while (!asyncOp.isDone)
            yield return null;
    }

    public void ChangeMenuSection(string animState)
    {
        menuAnim.SetTrigger(animState);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class FileUI
{
    public Text gardenName;
    public Text playedTime;
    public GameObject deleteButton;
    public GameObject newGameButton;
    public GameObject normalPlayButton;

    public void SetFileUI(SerializableData file)
    {
        deleteButton.SetActive(file != null);
        playedTime.gameObject.SetActive(file != null);
        newGameButton.gameObject.SetActive(file == null);
        normalPlayButton.SetActive(file != null);

        if (file != null)
        {
            gardenName.text = file.gardenName;
            TimeSpan timeSpan = TimeSpan.FromSeconds(file.spentTime);
            playedTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
