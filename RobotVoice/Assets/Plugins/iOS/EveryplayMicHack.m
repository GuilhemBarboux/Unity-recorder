#import "EveryplayMicHack.h"

void SetPreferredSampleRate(int sampleRate) {
    AVAudioSession *audioSession = [AVAudioSession sharedInstance];
    [audioSession setPreferredHardwareSampleRate:sampleRate error:nil];
}