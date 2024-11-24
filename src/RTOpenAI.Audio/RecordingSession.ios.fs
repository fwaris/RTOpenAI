namespace RTOpenAI.Audio.Ios

#if IOS
open AVFoundation
open Foundation

module internal RecordingSession =
    let checkError src f =
        let err : NSError = f()
        if err <> null then
            let msg = $"error with recording session. Src {src}; Error {err}"
            failwith msg                   
            
    let activate() =
        let sess = AVAudioSession.SharedInstance()
        checkError "activate:SetCategory" (fun () -> sess.SetCategory(AVAudioSessionCategory.Record))
        checkError "activate:SetActive" (fun () -> sess.SetActive(true))
            
    let release() =
        let sess = AVAudioSession.SharedInstance()
        checkError "release" (fun () -> sess.SetActive(false))
        
(*
{
    internal static void InitializeSession(BaseOptions options)
    {		
        var audioSession = AVAudioSession.SharedInstance();

		var error = audioSession.SetCategory(options.Category, options.Mode, options.CategoryOptions);
		if (error is not null)
		{
			Trace.TraceError("failed to set category");
			Trace.TraceError(error.ToString());
		}

		error = audioSession.SetActive(true, GetSessionSetActiveOptions(options));
		if (error is not null)
		{
			Trace.TraceError("failed activate audio session");
			Trace.TraceError(error.ToString());
		}
    }

    public static void FinishSession(BaseOptions options)
    {
        if (options.SessionLifetime is not SessionLifetime.KeepSessionAlive)
		{
			var audioSession = AVAudioSession.SharedInstance();

			var error = audioSession.SetActive(false, GetSessionSetActiveOptions(options));
			if (error is not null)
			{
				Trace.WriteLine($"Failed to deactivate the audio session: {error}");
			}
		}
    }

    private static AVAudioSessionSetActiveOptions GetSessionSetActiveOptions(BaseOptions options)
    {
        if (options.SessionLifetime is SessionLifetime.EndSessionAndNotifyOthers)
        {
            return AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation;
        }
        else
        {
            return 0;
        }
    }
}
*)

#endif
