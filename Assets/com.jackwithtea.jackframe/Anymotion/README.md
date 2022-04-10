# Anymotion
## Introduction
Use Unity Playable API instead of Animator Controller(and Animator Controller Override).  
Anymotion can crossfade between any two motions.  

## Quick Start
#### 1. Ready
1.1 AddComponent Anymotion to GameObject that is has Animator  
1.2 Anymotion.RegisterClip(int clipID, AnimationClip clip);  
1.3 Anymotion.AddMixer(int mixerID, float weight);  

#### 2. Usage
```
// 【Instant Play】
// No trasition time
Anymotion.Play(int mixerID, int clipID);

// 【Crossfade】
Anymotion.Crossfade(int mixerID, int clipID, float duration);
```

#### Demo
Demo - Anymotion.Play  
![Play](./Document~/play.gif)  

Demo - Anymotion.Crossfade  
![Crossfade](./Document~/crossfade.gif)  