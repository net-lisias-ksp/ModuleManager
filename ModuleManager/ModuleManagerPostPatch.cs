﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using ModuleManager.Extensions;
using ModuleManager.Logging;

namespace ModuleManager
{
    public delegate void ModuleManagerPostPatchCallback();

    public class PostPatchLoader : LoadingSystem
    {
        public static PostPatchLoader Instance { get; private set; }

        private static readonly List<ModuleManagerPostPatchCallback> postPatchCallbacks = new List<ModuleManagerPostPatchCallback>();

        private readonly IBasicLogger logger = new ModLogger("ModuleManager", new UnityLogger(UnityEngine.Debug.unityLogger));

        private bool ready = false;

        public static void AddPostPatchCallback(ModuleManagerPostPatchCallback callback)
        {
            if (!postPatchCallbacks.Contains(callback))
                postPatchCallbacks.Add(callback);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public override bool IsReady() => ready;

        public override float LoadWeight() => 0;

        public override float ProgressFraction() => 1;

        public override string ProgressTitle() => "ModuleManager : post patch";

        public override void StartLoad()
        {
            ready = false;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

#if DEBUG
            InGameTestRunner testRunner = new InGameTestRunner(logger);
            testRunner.RunTestCases(GameDatabase.Instance.root);
#endif

            yield return null;

            logger.Info("Reloading resources definitions");
            PartResourceLibrary.Instance.LoadDefinitions();

            logger.Info("Reloading Trait configs");
            GameDatabase.Instance.ExperienceConfigs.LoadTraitConfigs();

            logger.Info("Reloading Part Upgrades");
            PartUpgradeManager.Handler.FillUpgrades();

            yield return null;

            logger.Info("Running post patch callbacks");

            foreach (ModuleManagerPostPatchCallback callback in postPatchCallbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception e)
                {
                    logger.Exception("Exception while running a post patch callback", e);
                }
                yield return null;
            }
            yield return null;

            // Call all "public static void ModuleManagerPostLoad()" on all class
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in ass.GetTypes())
                    {
                        MethodInfo method = type.GetMethod("ModuleManagerPostLoad", BindingFlags.Public | BindingFlags.Static);

                        if (method != null && method.GetParameters().Length == 0)
                        {
                            try
                            {
                                logger.Info("Calling " + ass.GetName().Name + "." + type.Name + "." + method.Name + "()");
                                method.Invoke(null, null);
                            }
                            catch (Exception e)
                            {
                                logger.Exception("Exception while calling " + ass.GetName().Name + "." + type.Name + "." + method.Name + "()", e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Exception("Post run call threw an exception in loading " + ass.FullName, e);
                }
            }

            yield return null;

            // Call "public void ModuleManagerPostLoad()" on all active MonoBehaviour instance
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                MethodInfo method = obj.GetType().GetMethod("ModuleManagerPostLoad", BindingFlags.Public | BindingFlags.Instance);

                if (method != null && method.GetParameters().Length == 0)
                {
                    try
                    {
                        logger.Info("Calling " + obj.GetType().Name + "." + method.Name + "()");
                        method.Invoke(obj, null);
                    }
                    catch (Exception e)
                    {
                        logger.Exception("Exception while calling " + obj.GetType().Name + "." + method.Name + "() :\n", e);
                    }
                }
            }

            yield return null;

            if (ModuleManager.dumpPostPatch)
                ModuleManager.OutputAllConfigs();

            stopwatch.Stop();
            logger.Info("Post patch ran in " + ((float)stopwatch.ElapsedMilliseconds / 1000).ToString("F3") + "s");

            ready = true;
        }
    }
}
