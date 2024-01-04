using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Sounds {
    public class SoundManager : MonoBehaviour {
        public static SoundManager Inst { get; private set; }

        [Header("Setup")]
        [SerializeField] private AudioMixerGroup defaultMixerGroup;
        public AudioMixerGroup DefaultMixerGroup => defaultMixerGroup;

        private List<AudioSource> activeSources = new List<AudioSource>();
        private Transform sounds3DContainer = null;

        private void Awake() {
            // Singleton handling
            if (Inst) {
                Destroy(gameObject);
                return;
            } else {
                // Dont destroy on load handled by the ManagersInit script
                Inst = this;
            }

            // Create a 3D sounds container game object
            sounds3DContainer = new GameObject("Sounds 3D").transform;
        }

        private void Update() {
            // Remove non-active audio sources
            for (int i = 0; i < activeSources.Count; i++) {
                AudioSource src = activeSources[i];
                if (!src.isPlaying) {
                    activeSources.RemoveAt(i--);
                    Destroy(src);
                }
            }
        }

        public AudioSource Play(AudioClip clip, AudioMixerGroup mixerGroup = null, float volume = 1f, bool loop = false, float delay = 0f, float pitch = 1f) {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.outputAudioMixerGroup = mixerGroup != null ? mixerGroup : defaultMixerGroup; ;
            src.clip = clip;
            src.volume = volume;
            src.loop = loop;
            src.pitch = pitch;
            src.PlayDelayed(delay);
            activeSources.Add(src);
            return src;
        }

        /// <summary>
        /// Stops a playing sound (AudioSource).
        /// </summary>
        /// <param name="source">The AudioSource that was retreived by the Play() method</param>
        /// <returns>true if the sound was stopped, false if the sound was not active and therefore not stoppped</returns>
        public bool Stop(AudioSource source) {
            AudioSource src = activeSources.Find(src => src == source);
            if (!src) return false;
            src.Stop();
            return true;
        }

        /// <summary>
        /// Stop a playing sound (AudioSource) by clip. Use with caution, because this can stop audio sources that were triggered by other origins.
        /// </summary>
        /// <param name="clip">audio clip to search and stop (only the first found)</param>
        /// <returns>true if the clip was stopped, false if the clip was not active and therefore not stopped</returns>
        public bool Stop(AudioClip clip) {
            AudioSource src = activeSources.Find(src => src.clip == clip);
            if (!src) return false;
            src.Stop();
            return true;
        }

        /// <summary>
        /// Play a 3D sound at a certain position in world space. Sounds started with this method cannot be stoppped.
        /// </summary>
        /// <param name="name">Name of the sound</param>
        /// <param name="position">Location the sound is spawned at</param>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="mixerGroup">The mixer group the audio signal is routed to</param>
        /// <param name="volume">Volume of the sound</param>
        /// <param name="loop">Should the sound be looped?</param>
        /// <param name="delay">Delay before playing the sound</param>
        /// <param name="pitch">Sound pitch</param>
        /// <returns>The newly spawned AudioSource component on a new gameobject that plays the sound</returns>
        public AudioSource PlayAtPosition(string name, Vector3 position, AudioClip clip, AudioMixerGroup mixerGroup = null, float volume = 1f, bool loop = false, float delay = 0f, float pitch = 1f) {
            // Instantiate a sound game object
            GameObject audioObj = new GameObject("Dynamic Sound " + name, typeof(AudioSource));
            audioObj.transform.position = position;
            audioObj.transform.parent = sounds3DContainer;

            // Set some AudioSource params
            AudioSource src = audioObj.GetComponent<AudioSource>();
            src.clip = clip;
            src.outputAudioMixerGroup = mixerGroup != null ? mixerGroup : defaultMixerGroup; ;
            src.playOnAwake = false;
            src.volume = volume;
            src.loop = loop;
            src.pitch = pitch;
            src.spatialBlend = 1f;
            src.dopplerLevel = 0f;
            src.PlayDelayed(delay);
            Destroy(audioObj, clip.length * (1f / pitch));

            return src;
        }
    }
}