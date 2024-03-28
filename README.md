# TD3-based_UAV_collision_avoidance
code for `Autonomous navigation of UAV in multi-obstacle environments based on a Deep Reinforcement Learning approach' [Google Scholar](https://scholar.google.com.hk/scholar?hl=en&as_sdt=0,29&q=Autonomous+navigation+of+UAV+in+multi-obstacle+environments+based+on+a+Deep+Reinforcement+Learning+approach&btnG=)

Video: [YouTube](https://youtu.be/1zL-srwnoZE?si=JErQlxk8PZ6JdZqP)

# Dependencies
* Unity 2019.3.2f1
* Unity Hub 2.3.2
* [ml-agents](https://unity.com/products/machine-learning-agents) 0.15.1
* Anaconda
* numpy, torch, gym_unity, etc.

# Tutorials
* [在Unity環境中訓練強化學習AI！(YouTube)](https://www.youtube.com/playlist?list=PLDV2CyUo4q-I3zmaqisW5xAANFHgKnJfD)
* [ml-agnets/docs](https://github.com/Unity-Technologies/ml-agents/tree/latest_release/docs)
* [Unity Scripting Reference](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/index.html)
* [Unity ML-Agents forum](https://forum.unity.com/forums/ml-agents.453/)

# Materials
* **UAV Project:** \`Drone Controller Full' \(now unavailable, and replacement could be \`[Drone Controller Pro](https://assetstore.unity.com/packages/tools/physics/drone-controller-pro-111163#content)'\) in Unity Asset Store.
* **TD3:** [Policy-Gradient-Methods](https://github.com/cyoon1729/Policy-Gradient-Methods)

# How to run
open `rl/TD3_original/TD3_original.ipynb` in jupyternotebook and run it.  
> Notice:  
> Cell 3 is only for examining if the environment works well  
> Cell 5 is only for training  
> Cell 9 is only for testing

If you wish to modify the environment settings, including DRL settings (actions, observations, rewards, terminal conditions), collision detection, laser ranging, etc, you can find them in `Assets/RocketAgent.cs`.  
Or if you're interested in creating a new environment, the guide `[Learning-Environment-Create-New](https://github.com/Unity-Technologies/ml-agents/blob/latest_release/docs/Learning-Environment-Create-New.md)' could be helpful.
