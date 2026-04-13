using UnityEngine;

namespace BroMakerLib.Extensions
{
    public static class AudioClipExtensions
    {
        /// <summary>Returns a shallow copy of the array. Safe on null (returns null).
        /// Use when caching <see cref="AudioClip"/> arrays from a prefab's SoundHolder so that
        /// accidental writes through the cached field cannot mutate the shared prefab data.</summary>
        /// <param name="source">Source array (typically from a prefab's SoundHolder field).</param>
        /// <returns>A new array containing the same AudioClip references, or null if source was null.</returns>
        public static AudioClip[] CloneArray(this AudioClip[] source)
        {
            return source?.Clone() as AudioClip[];
        }
    }
}
