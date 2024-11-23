using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

public static class PlayableDirectorExtensions
{
    public static UniTask WaitForEnd(this PlayableDirector director)
    {
        var tcs = new UniTaskCompletionSource();

        void OnPlayableDirectorStopped(PlayableDirector d)
        {
            director.stopped -= OnPlayableDirectorStopped;
            tcs.TrySetResult();
        }

        director.stopped += OnPlayableDirectorStopped;
        return tcs.Task;
    }
}