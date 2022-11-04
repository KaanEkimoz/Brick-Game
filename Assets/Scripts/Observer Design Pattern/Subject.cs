using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subject : MonoBehaviour
{
    private List<Observer> observers = null;

    [SerializeField] private SubjectType subjectType;
    public SubjectType SubjectType => subjectType;

    public void RegisterObserver(Observer observer)
    {
        if (observers == null)
            observers = new List<Observer>();
        observers.Add(observer);
    }

    private void Start()
    {
        ObserverManager.Instance.RegisterSubject(this);
    }
}
