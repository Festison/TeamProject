using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Veco
{

    public class SoundManager : SingleTon<SoundManager>, IAddressable
    {
        public AudioMixer mixer;
        public GameObject soundObj;
        [SerializeField] AudioSource bgSound;
        [SerializeField] AudioClip[] bgClips;
        [SerializeField] public AudioClip[] sfxClips;
        AsyncOperationHandle handle;

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            bgSound = GetComponent<AudioSource>();
            
            BgSoundPlay(bgClips[0]);
        }


        private void Update()
        {
            //transform.position = Camera.main.transform.position;

        }

        //�� ��ȯ ��, ����� �ٲ�
        void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if(scene.name == "LOBBY") 
            {
                
            }
            else if(scene.name == "InGAME")
            {

            }
        }

        public void SFXPlay(string clipAddress)     //SFX Play
        {
            GameObject soundObj = ObjectPoolManager.Instance.PopObj(this.soundObj, transform.position, transform.rotation);
            soundObj.SetActive(true);
            if (soundObj.TryGetComponent(out IPlayClipable play))
                play.SoundPlay(clipAddress, mixer);
            else
                Debug.Log("Cannot refer to the IPlayClipable Interface");
        }

        public void BgSoundPlay(AudioClip clip)     //BgSound Play
        {
            bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BGSound")[0];
            LoadAsset(clip.name);
            bgSound.loop = true;
            bgSound.volume = 0.1f;
        }

        public void BgSoundVolume(float value)          //BGsound Volume Setting
        {
            mixer.SetFloat("BGSoundVolume", Mathf.Log10(value) * 20);
        }

        public void SFXVolume(float value)                 //SFX Volume Setting
        {
            mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        }

        public void LoadAsset(string namePath)
        {
            Addressables.LoadAssetAsync<AudioClip>("BGM").Completed += (clip) =>
            {
                handle = clip;
                bgSound.clip = clip.Result;
                bgSound.Play();
            };
        }

        public void UnloadAsset()
        {
            bgSound.clip = null;
            Addressables.Release(handle);
        }
    }
}
