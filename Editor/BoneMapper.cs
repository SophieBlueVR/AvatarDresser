/*

AvatarDresser - a simple script to apply an item of clothing to your avatar.

Copyright (c) 2022 SophieBlue

*/

using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEditor;
using UnityEngine;

namespace SophieBlue.AvatarDresser {

    internal class BoneMapper {

        // This list is originally from
        // https://github.com/HhotateA/AvatarModifyTools/blob/d8ae75fed8577707253d6b63a64d6053eebbe78b/Assets/HhotateA/AvatarModifyTool/Editor/EnvironmentVariable.cs#L81-L139
        // Copyright (c) 2021 @HhotateA_xR
        // Licensed under the MIT License
        private static string[][] boneNamePatterns = new[] {
            new[] {"Hips", "Hip"},
            new[] {"LeftUpperLeg", "UpperLeg_Left", "UpperLeg_L", "Leg_Left", "Leg_L"},
            new[] {"RightUpperLeg", "UpperLeg_Right", "UpperLeg_R", "Leg_Right", "Leg_R"},
            new[] {"LeftLowerLeg", "LowerLeg_Left", "LowerLeg_L", "Knee_Left", "Knee_L"},
            new[] {"RightLowerLeg", "LowerLeg_Right", "LowerLeg_R", "Knee_Right", "Knee_R"},
            new[] {"LeftFoot", "Foot_Left", "Foot_L"},
            new[] {"RightFoot", "Foot_Right", "Foot_R"},
            new[] {"Spine"},
            new[] {"Chest"},
            new[] {"Neck"},
            new[] {"Head"},
            new[] {"LeftShoulder", "Shoulder_Left", "Shoulder_L"},
            new[] {"RightShoulder", "Shoulder_Right", "Shoulder_R"},
            new[] {"LeftUpperArm", "UpperArm_Left", "UpperArm_L", "Arm_Left", "Arm_L"},
            new[] {"RightUpperArm", "UpperArm_Right", "UpperArm_R", "Arm_Right", "Arm_R"},
            new[] {"LeftLowerArm", "LowerArm_Left", "LowerArm_L"},
            new[] {"RightLowerArm", "LowerArm_Right", "LowerArm_R"},
            new[] {"LeftHand", "Hand_Left", "Hand_L"},
            new[] {"RightHand", "Hand_Right", "Hand_R"},
            new[] {"LeftToes", "Toes_Left", "Toe_Left", "ToeIK_L", "Toes_L", "Toe_L"},
            new[] {"RightToes", "Toes_Right", "Toe_Right", "ToeIK_R", "Toes_R", "Toe_R"},
            new[] {"LeftEye", "Eye_Left", "Eye_L"},
            new[] {"RightEye", "Eye_Right", "Eye_R"},
            new[] {"Jaw"},
            new[] {"LeftThumbProximal", "ProximalThumb_Left", "ProximalThumb_L"},
            new[] {"LeftThumbIntermediate", "IntermediateThumb_Left", "IntermediateThumb_L"},
            new[] {"LeftThumbDistal", "DistalThumb_Left", "DistalThumb_L"},
            new[] {"LeftIndexProximal", "ProximalIndex_Left", "ProximalIndex_L"},
            new[] {"LeftIndexIntermediate", "IntermediateIndex_Left", "IntermediateIndex_L"},
            new[] {"LeftIndexDistal", "DistalIndex_Left", "DistalIndex_L"},
            new[] {"LeftMiddleProximal", "ProximalMiddle_Left", "ProximalMiddle_L"},
            new[] {"LeftMiddleIntermediate", "IntermediateMiddle_Left", "IntermediateMiddle_L"},
            new[] {"LeftMiddleDistal", "DistalMiddle_Left", "DistalMiddle_L"},
            new[] {"LeftRingProximal", "ProximalRing_Left", "ProximalRing_L"},
            new[] {"LeftRingIntermediate", "IntermediateRing_Left", "IntermediateRing_L"},
            new[] {"LeftRingDistal", "DistalRing_Left", "DistalRing_L"},
            new[] {"LeftLittleProximal", "ProximalLittle_Left", "ProximalLittle_L"},
            new[] {"LeftLittleIntermediate", "IntermediateLittle_Left", "IntermediateLittle_L"},
            new[] {"LeftLittleDistal", "DistalLittle_Left", "DistalLittle_L"},
            new[] {"RightThumbProximal", "ProximalThumb_Right", "ProximalThumb_R"},
            new[] {"RightThumbIntermediate", "IntermediateThumb_Right", "IntermediateThumb_R"},
            new[] {"RightThumbDistal", "DistalThumb_Right", "DistalThumb_R"},
            new[] {"RightIndexProximal", "ProximalIndex_Right", "ProximalIndex_R"},
            new[] {"RightIndexIntermediate", "IntermediateIndex_Right", "IntermediateIndex_R"},
            new[] {"RightIndexDistal", "DistalIndex_Right", "DistalIndex_R"},
            new[] {"RightMiddleProximal", "ProximalMiddle_Right", "ProximalMiddle_R"},
            new[] {"RightMiddleIntermediate", "IntermediateMiddle_Right", "IntermediateMiddle_R"},
            new[] {"RightMiddleDistal", "DistalMiddle_Right", "DistalMiddle_R"},
            new[] {"RightRingProximal", "ProximalRing_Right", "ProximalRing_R"},
            new[] {"RightRingIntermediate", "IntermediateRing_Right", "IntermediateRing_R"},
            new[] {"RightRingDistal", "DistalRing_Right", "DistalRing_R"},
            new[] {"RightLittleProximal", "ProximalLittle_Right", "ProximalLittle_R"},
            new[] {"RightLittleIntermediate", "IntermediateLittle_Right", "IntermediateLittle_R"},
            new[] {"RightLittleDistal", "DistalLittle_Right", "DistalLittle_R"},
            new[] {"UpperChest"},
        };

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

            NameToBoneMap = nameToBoneMap.ToImmutableDictionary();
            BoneToNameMap = boneToNameMap.ToImmutableDictionary();
        }

        internal void RegisterNameForBone(string name, HumanBodyBones bone) {
            nameToBoneMap[name] = bone;
            if (!boneToNameMap.TryGetValue(bone, out var names)) {
                names = ImmutableList<string>.Empty;
            }

            if (!names.Contains(name)) {
                boneToNameMap[bone] = names.Add(name);
            }
        }

        string TryFindBone(string sourceBone, List bones, out var targetBone) {
            // convert the given bone list to normalized names for better matching later
            var normalizedBoneList = new List<string>();
            foreach (var name in bones) {
                normalizedBoneList.Add(NormalizeName(name));
            }

            // Try to map the given name to the standard name
            if (NameToBoneMap.TryGetValue(NormalizeName(sourceBone), out var sourceName)) {

                // then try to find the right bone in the given bone list
                foreach (var testName in BoneToNameMap[sourceName]) {
                    if (normalizedBoneList.Contains(testName)) {
                        targetBone = testName;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
