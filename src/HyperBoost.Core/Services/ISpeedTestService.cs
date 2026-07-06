// Standardized to production level
// Purpose: Interface for executing Wi-Fi speed tests (Ping, Jitter, Download, Upload)
// Dependencies: System, System.Threading.Tasks, HyperBoost.Core.Models

namespace HyperBoost.Core.Services;

using System;
using System.Threading.Tasks;
using HyperBoost.Core.Models;

public interface ISpeedTestService
{
    event EventHandler<SpeedTestStatus>? OnStatusChanged;
    Task<SpeedTestStatus> RunSpeedTestAsync();
}
