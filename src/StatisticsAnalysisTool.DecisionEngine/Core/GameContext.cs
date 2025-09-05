using System;
using System.Collections.Generic;
using System.Drawing;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;

namespace StatisticsAnalysisTool.DecisionEngine.Core
{
    /// <summary>
    /// Implementação básica de GameContext
    /// </summary>
    public class GameContext : IGameContext
    {
        private readonly Dictionary<string, object> _values = new();
        private readonly HashSet<string> _flags = new();
        
        public string CurrentState { get; set; } = "IDLE";
        public Point PlayerPosition { get; set; }
        public int PlayerHealth { get; set; } = 100;
        public int PlayerEnergy { get; set; } = 100;
        
        public bool HasFlag(string flag) => _flags.Contains(flag);
        
        public void SetFlag(string flag, bool value)
        {
            if (value)
                _flags.Add(flag);
            else
                _flags.Remove(flag);
        }
        
        public bool TryGetValue<T>(string key, out T value)
        {
            if (_values.TryGetValue(key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }
            value = default(T);
            return false;
        }
        
        public void SetValue(string key, object value) => _values[key] = value;
        
        public IReadOnlyDictionary<string, object> GetAllValues() => _values;
        
        public IReadOnlySet<string> GetAllFlags() => _flags;
        
        public void Clear()
        {
            _values.Clear();
            _flags.Clear();
            CurrentState = "IDLE";
            PlayerPosition = Point.Empty;
            PlayerHealth = 100;
            PlayerEnergy = 100;
        }
        
        public IGameContext Clone()
        {
            var clone = new GameContext
            {
                CurrentState = CurrentState,
                PlayerPosition = PlayerPosition,
                PlayerHealth = PlayerHealth,
                PlayerEnergy = PlayerEnergy
            };
            
            foreach (var kvp in _values)
            {
                clone._values[kvp.Key] = kvp.Value;
            }
            
            foreach (var flag in _flags)
            {
                clone._flags.Add(flag);
            }
            
            return clone;
        }
    }
}
