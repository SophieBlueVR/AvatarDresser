/*

AvatarDresser - a simple script to apply an item of clothing to your avatar.

Copyright (c) 2022 SophieBlue

*/

using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace SophieBlue.AvatarDresser {

    internal class BoneMapper {

        // This list is originally from
        // https://github.com/HhotateA/AvatarModifyTools/blob/d8ae75fed8577707253d6b63a64d6053eebbe78b/Assets/HhotateA/AvatarModifyTool/Editor/EnvironmentVariable.cs#L81-L139
        // Copyright (c) 2021 @HhotateA_xR
        // Licensed under the MIT License
        private static string[][] boneNamePatterns = new[] {
            new[] {"Hips", "Hip"},

            // legs
            new[] {"LeftUpperLeg", "UpperLeg_Left", "UpperLeg_L", "Leg_Left", "Leg_L", "Thigh_L", "Left_Leg"},
            new[] {"RightUpperLeg", "UpperLeg_Right", "UpperLeg_R", "Leg_Right", "Leg_R", "Thigh_R", "Right_Leg"},
            new[] {"LeftLowerLeg", "LowerLeg_Left", "LowerLeg_L", "Knee_Left", "Left_Knee", "Knee_L", "Shin_L"},
            new[] {"RightLowerLeg", "LowerLeg_Right", "LowerLeg_R", "Knee_Right", "Right_Knee", "Knee_R", "Shin_R"},
            new[] {"LeftFoot", "Foot_Left", "Foot_L", "LeftAnkle", "Ankle_L"},
            new[] {"RightFoot", "Foot_Right", "Foot_R", "RightAnkle", "Ankle_R"},

            new[] {"Spine"},
            new[] {"Chest"},
            new[] {"UpperChest"},
            new[] {"Neck"},
            new[] {"Head"},

            // arms
            new[] {"LeftShoulder", "Shoulder_Left", "Shoulder_L"},
            new[] {"RightShoulder", "Shoulder_Right", "Shoulder_R"},
            new[] {"LeftUpperArm", "UpperArm_Left", "UpperArm_L", "Arm_Left", "Arm_L"},
            new[] {"RightUpperArm", "UpperArm_Right", "UpperArm_R", "Arm_Right", "Arm_R"},
            new[] {"LeftLowerArm", "LowerArm_Left", "LowerArm_L", "Forearm_L", "LeftElbow", "Elbow_L"},
            new[] {"RightLowerArm", "LowerArm_Right", "LowerArm_R", "Forearm_R", "RightElbow", "Elbow_R"},
            new[] {"LeftHand", "Hand_Left", "Hand_L", "Wrist_L"},
            new[] {"RightHand", "Hand_Right", "Hand_R", "Wrist_R"},
            new[] {"LeftToes", "Toes_Left", "Toe_Left", "ToeIK_L", "Toes_L", "Toe_L"},
            new[] {"RightToes", "Toes_Right", "Toe_Right", "ToeIK_R", "Toes_R", "Toe_R"},
            new[] {"LeftEye", "Eye_Left", "Eye_L"},
            new[] {"RightEye", "Eye_Right", "Eye_R"},
            new[] {"Jaw"},

            // left fingers
            new[] {"LeftThumbProximal", "ProximalThumb_Left", "ProximalThumb_L"},
            new[] {"LeftThumbIntermediate", "IntermediateThumb_Left", "IntermediateThumb_L"},
            new[] {"LeftThumbDistal", "DistalThumb_Left", "DistalThumb_L"},
            new[] {"LeftIndexProximal", "ProximalIndex_Left", "ProximalIndex_L", "f_index.01.L"},
            new[] {"LeftIndexIntermediate", "IntermediateIndex_Left", "IntermediateIndex_L", "f_index.02.L"},
            new[] {"LeftIndexDistal", "DistalIndex_Left", "DistalIndex_L", "f_index.03.L"},
            new[] {"LeftMiddleProximal", "ProximalMiddle_Left", "ProximalMiddle_L",
                   "LeftMiddleFinger1", "f_middle.01.L"},
            new[] {"LeftMiddleIntermediate", "IntermediateMiddle_Left", "IntermediateMiddle_L",
                   "LeftMiddleFinger2", "f_middle.02.L", "f_middle_3.L"},
            new[] {"LeftMiddleDistal", "DistalMiddle_Left", "DistalMiddle_L",
                   "LeftMiddleFinter3", "f_middle.03.L", "f_middle_3.L"},
            new[] {"LeftRingProximal", "ProximalRing_Left", "ProximalRing_L",
                   "f_ring.01.L", "f_ring_1.L"},
            new[] {"LeftRingIntermediate", "IntermediateRing_Left", "IntermediateRing_L",
                   "f_ring.02.L", "f_ring_2.L"},
            new[] {"LeftRingDistal", "DistalRing_Left", "DistalRing_L",
                   "f_ring.03.L", "f_ring_3.L"},
            new[] {"LeftLittleProximal", "ProximalLittle_Left", "ProximalLittle_L",
                   "f_little.01.L", "f_pinky.01.L", "f_pinky_1.L"},
            new[] {"LeftLittleIntermediate", "IntermediateLittle_Left", "IntermediateLittle_L",
                   "f_little.02.L", "f_pinky.01.L", "f_pinky_2.L"},
            new[] {"LeftLittleDistal", "DistalLittle_Left", "DistalLittle_L",
                   "f_little.03.L", "f_pinky.03.L", "f_pinky_3.L"},

            // right fingers
            new[] {"RightThumbProximal", "ProximalThumb_Right", "ProximalThumb_R",
                   "thumb.01.R", "FThumb1.R"},
            new[] {"RightThumbIntermediate", "IntermediateThumb_Right", "IntermediateThumb_R",
                   "thumb.02.R", "FThumb2.R"},
            new[] {"RightThumbDistal", "DistalThumb_Right", "DistalThumb_R",
                   "thumb.03.R", "FThumb3.R"},
            new[] {"RightIndexProximal", "ProximalIndex_Right", "ProximalIndex_R", "f_index.01.R"},
            new[] {"RightIndexIntermediate", "IntermediateIndex_Right", "IntermediateIndex_R", "f_index.02.R"},
            new[] {"RightIndexDistal", "DistalIndex_Right", "DistalIndex_R", "f_index.03.R"},
            new[] {"RightMiddleProximal", "ProximalMiddle_Right", "ProximalMiddle_R", "f_middle.01.R"},
            new[] {"RightMiddleIntermediate", "IntermediateMiddle_Right", "IntermediateMiddle_R", "f_middle.02.R"},
            new[] {"RightMiddleDistal", "DistalMiddle_Right", "DistalMiddle_R", "f_middle.03.R"},
            new[] {"RightRingProximal", "ProximalRing_Right", "ProximalRing_R", "f_ring.01.R"},
            new[] {"RightRingIntermediate", "IntermediateRing_Right", "IntermediateRing_R", "f_ring.02.R"},
            new[] {"RightRingDistal", "DistalRing_Right", "DistalRing_R", "f_ring.03.R"},
            new[] {"RightLittleProximal", "ProximalLittle_Right", "ProximalLittle_R",
                   "f_little.01.R", "f_pinky.01.R", "f_pinky_1.L"},
            new[] {"RightLittleIntermediate", "IntermediateLittle_Right", "IntermediateLittle_R",
                   "f_little.02.R", "f_pinky.02.R", "f_pinky_2.L"},
            new[] {"RightLittleDistal", "DistalLittle_Right", "DistalLittle_R",
                   "f_little.03.R", "f_pinky.03.R", "f_pinky_3.L"},
        };


        /*
            new[] {"Breast_L"},
            new[] {"Breast_R"},
            new[] {"Tail", "TailRoot", "Tail_1", "Tail_001"},
        */

        internal static string NormalizeName(string name) {
            return name.ToLowerInvariant()
                .Replace("_", "")
                .Replace(".", "")
                .Replace(" ", "");
        }

        internal static readonly ImmutableDictionary<string, HumanBodyBones> NameToBoneMap;
        internal static readonly ImmutableDictionary<HumanBodyBones, ImmutableList<string>> BoneToNameMap;

        static BoneMapper() {
            var nameToBoneMap = new Dictionary<string, HumanBodyBones>();
            var boneToNameMap = new Dictionary<HumanBodyBones, ImmutableList<string>>();

            for (int i = 0; i < boneNamePatterns.Length; i++) {
                var bone = (HumanBodyBones) i;
                foreach (var name in boneNamePatterns[i]) {
                    RegisterNameForBone(NormalizeName(name), bone);
                }
            }

            void RegisterNameForBone(string name, HumanBodyBones bone) {
                nameToBoneMap[name] = bone;
                if (!boneToNameMap.TryGetValue(bone, out var names)) {
                    names = ImmutableList<string>.Empty;
                }

                if (!names.Contains(name)) {
                    boneToNameMap[bone] = names.Add(name);
                }
            }

            NameToBoneMap = nameToBoneMap.ToImmutableDictionary();
            BoneToNameMap = boneToNameMap.ToImmutableDictionary();
        }

        public static bool TryFindBone(string sourceBone, Dictionary<string, Transform> bones, out Transform targetBone) {
            // convert the given bone list to normalized names for better matching
            var normalizedBoneList = new Dictionary<string, string>();
            foreach (var boneName in bones.Keys) {
                normalizedBoneList.Add(NormalizeName(boneName), boneName);
            }

            // Try to map the given name to the standard name
            if (NameToBoneMap.TryGetValue(NormalizeName(sourceBone), out var sourceName)) {

                // then try to find the right bone in the given bone list
                foreach (var testName in BoneToNameMap[sourceName]) {
                    if (normalizedBoneList.TryGetValue(testName, out var realName)) {
                        targetBone = bones[realName];
                        return true;
                    }
                }
            }
            targetBone = null;
            return false;
        }
    }
}
