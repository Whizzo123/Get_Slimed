using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/*
*AUTHOR: Tanapat Somrid
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: To provide a global access point to access pre-loaded sound files and play them from within the manager or from an outside object
*/


/// <summary>
/// This is a lazy audio manager which will play sounds on the go without any real 3d effect.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio floats")]
    //Dynamic Floats
    [SerializeField][Tooltip("Music Volume: Only for music")][Range(0, 1)] public float MusicVolume;
    [SerializeField][Tooltip("Sound Volume: Only for sounds")][Range(0, 1)] public float SoundVolume;
    [SerializeField][Tooltip("Master Volume: Multiplied by other volumes")][Range(0, 1)] public float MasterVolume;
    [Space]

    //Audio management
    [SerializeField][Tooltip("All sound is stored inside this")] public AudioClips[] SoundBank;
    [SerializeField][Tooltip("All music is stored inside this")] public AudioClips[] MusicBank;

    /// <summary>
    /// AudioClips is used to store details about a track. It has:
    /// <list type="bullet">
    ///<listheader>
    ///    <item>Clip name</item>
    ///    <item>Audio clip</item><description>- The actual sound file</description>
    ///    <item>Clip volume</item>
    ///    <item>Loop?</item>
    ///    <item>Music?</item>
    ///</listheader>
    ///</list>
    ///<para>It's unity readable form is stored in AudioSource</para>
    ///<para>This should then be stored in an array and can be found and
    ///played from there thanks to these details</para>
    /// </summary>
    [System.Serializable]
    public class AudioClips
    {
        //Settings of the AudioClip
        [SerializeField][Tooltip("Name to call Play")] public string Name;
        [SerializeField][Tooltip("Adjust the volume of this individual sound")] public float Volume = 1;
        [SerializeField][Tooltip("Does this clip be looping?")] public bool Loop;

        [SerializeField][Tooltip("The audio file is stored here")] public AudioClip Clip;
        [Tooltip("The audio source")] public AudioSource Source;
    };


    public static AudioManager instance;

    /// <summary>
    /// Destroys self if duplicate. //Loads all audio files in automatically. 
    /// </summary>
    private void Awake()
    {
        //TODO: Load all sfx by file automatically

        //Creates a new audiomanager if there isn't one already
        if (instance == null) 
        {
            instance = this; 
        }
        else 
        { 
            Destroy(this.gameObject); 
            return; 
        }


        //Has to set the settings of AudioClips to the AudioSource in AudioClips
        foreach (AudioClips audio in SoundBank)
        {
            audio.Source = gameObject.AddComponent<AudioSource>();
            audio.Source.clip = audio.Clip;

            audio.Source.volume = audio.Volume;
            audio.Source.loop = audio.Loop;
        }        
        //Has to set the settings of AudioClips to the AudioSource in AudioClips
        foreach (AudioClips audio in MusicBank)
        {
            audio.Source = gameObject.AddComponent<AudioSource>();
            audio.Source.clip = audio.Clip;

            audio.Source.volume = audio.Volume;
            audio.Source.loop = audio.Loop;
        }
    }

    #region VOLUME_CONTROL
    public void ChangeMusicVolume(float Volume)
    {
        MusicVolume = Volume;
        UpdateMusicVolumes();
    }
    public void ChangeSoundVolume(float Volume)
    {
        SoundVolume = Volume;
        UpdateSoundVolumes();
    }
    public void ChangeMasterVolume(float Volume)
    {
        MasterVolume = Volume;
        UpdateSoundVolumes();
        UpdateMusicVolumes();
    }
    private void UpdateSoundVolumes()
    {
        foreach (AudioClips audio in SoundBank)
        {
                audio.Source.volume = SoundVolume * MasterVolume * audio.Volume;
        }
    }    
    private void UpdateMusicVolumes()
    {
        foreach (AudioClips audio in MusicBank)
        {
            audio.Source.volume = MusicVolume * MasterVolume * audio.Volume;
        }
    }
    #endregion

    #region PLAY
    /// <summary>
    /// Plays a sound effect "audioName"
    /// <para>Calling multiple times can result in sounds playing over eachother</para>
    /// </summary>
    public void PlaySound(string ClipName)
    {
        AudioClips AudioClip = Array.Find(SoundBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            AudioClip.Source.Play();
        }
    }
    public void PlaySoundFromSource(string ClipName, AudioSource NewSource)
    {
        AudioClips AudioClip = Array.Find(SoundBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            NewSource.clip = AudioClip.Clip;
            NewSource.volume = AudioClip.Volume;
            NewSource.loop = AudioClip.Loop;
            NewSource.Play();
        }
    }
    //This is unnecessary as each clip has it's own source. It will not have a new source to play non-exclusive sounds
    ///// <summary>
    ///// Plays a sound effect "audioName"
    ///// <para>Can be interupted by self</para>
    ///// </summary>
    //public void PlaySoundExclusive(string ClipName)
    //{
    //    AudioClips AudioClip = Array.Find(SoundBank, AudioClips => AudioClips.Name == ClipName);
    //    if (AudioClip != null)
    //    {
    //        //If sound is currently playing, will stop it and start again
    //        if (AudioClip.Source.isPlaying)
    //        {
    //            AudioClip.Source.Stop();
    //        }
    //        AudioClip.Source.Play();
    //    }
    //}
    ///<summary>
    /// Plays a sound effect "audioName"
    /// <para>Cannot be interupted by self</para>
    /// </summary>
    public void PlaySoundFull(string ClipName)
    {
        AudioClips AudioClip = Array.Find(SoundBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            //Will return if the sound is currently playing
            if (AudioClip.Source.isPlaying)
            {
                return;
            }
            AudioClip.Source.Play();
        }
    }

    public void PlayMusic(string ClipName)
    {
        AudioClips AudioClip = Array.Find(MusicBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            AudioClip.Source.Play();
        }
    }

    public void PlayMusicFull(string ClipName)
    {
        AudioClips AudioClip = Array.Find(MusicBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            //Will return if the sound is currently playing
            if (AudioClip.Source.isPlaying)
            {
                return;
            }
            AudioClip.Source.Play();
        }
    }
    #endregion



    #region STOP

    public void StopAllSound()
    {
        foreach (AudioClips Audio in SoundBank)
        {
            Audio.Source.Stop();
        }
    }
    public void StopSound(string ClipName)
    {
        AudioClips AudioClip = Array.Find(SoundBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            AudioClip.Source.Stop();
        }
    }    
    public void StopMusic(string ClipName)
    {
        AudioClips AudioClip = Array.Find(MusicBank, AudioClips => AudioClips.Name == ClipName);
        if (AudioClip != null)
        {
            AudioClip.Source.Stop();
        }
    }
    public void StopAllMusic()
    {
        foreach (AudioClips Audio in MusicBank)
        {
            Audio.Source.Stop();
        }
    }
    #endregion
}
