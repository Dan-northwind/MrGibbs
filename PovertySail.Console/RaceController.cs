﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Console
{
    public class RaceController:IRaceController
    {
        private State _state;
        private ILogger _logger;

        public RaceController(ILogger logger)
        {
            _state = new State();
            _logger = logger;
        }

        public State State
        {
            get
            {
                return _state;  
            }
        }

        public void CountdownAction()
        {
            lock (_state)
            {
                if (_state.StartTime.HasValue)
                {
                    if (_state.StartTime > _state.BestTime)
                    {
                        _logger.Info("Countdown Synced");
                        var remaining = _state.StartTime.Value - _state.BestTime;

                        if (remaining.Minutes >= 4)
                        {
                            _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 0, remaining.Seconds, remaining.Milliseconds));
                            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                        }
                        else if (remaining.Minutes >= 3)
                        {
                            _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 1, remaining.Seconds, remaining.Milliseconds));
                            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                        }
                        else
                        {
                            _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 0, remaining.Seconds, remaining.Milliseconds));
                            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                        }
                    }
                    else
                    {
                        _logger.Info("Countdown Reset");
                        _state.StartTime = null;
                        _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown reset");
                    }
                }
                else
                {
                    _logger.Info("Countdown Started");
                    _state.StartTime = _state.BestTime.AddMinutes(5);
                    _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown started");
                }
            }
        }

        public void SetCourseType(int index)
        {
            throw new NotImplementedException();
        }

        public void SetMarkLocation(MarkType markType)
        {
            lock (_state)
            {
                if (_state.Location != null)
                {
                    if (_state.Marks == null)
                    {
                        State.Marks = new List<Mark>();
                    }

                    if (!_state.Marks.Any())
                    {
                        var mark = new Mark() { MarkType = markType, CaptureMethod = MarkCaptureMethod.Location, Location = _state.Location };

                        State.Marks.Add(mark);
                        State.TargetMark = mark;
                    }
                    else if (_state.TargetMark != null && _state.TargetMark.MarkType == markType)
                    {
                        _state.TargetMark.CaptureMethod = MarkCaptureMethod.Location;
                        _state.TargetMark.Location = _state.Location;
                    }
                    else if (State.TargetMark != null && _state.TargetMark.MarkType != markType)
                    {
                        var mark = new Mark() { MarkType = markType, CaptureMethod = MarkCaptureMethod.Location, Location = _state.Location };

                        State.Marks.Add(mark);
                        //State.TargetMark = mark;
                    }
                    else
                    {
                        _logger.Error("User set mark location for " + markType + " but unsure what to do with it");
                    }
                }
            }
        }
        public void SetMarkBearing(MarkType markType, double bearing, bool magneticBearing)
        {
            lock (_state)
            {
                if (_state.Location != null)
                {
                    if (_state.Marks == null)
                    {
                        State.Marks = new List<Mark>();
                    }

                    Bearing fullBearing = new Bearing() { Location = _state.Location, RecordedAt = _state.BestTime, CompassHeading = bearing };

                    //compensate for magnetic deviation
                    if(magneticBearing && _state.MagneticDeviation.HasValue)
                    {
                        fullBearing.CompassHeading = fullBearing.CompassHeading + _state.MagneticDeviation.Value;
                    }

                    _logger.Info(string.Format("Received bearing for {0} of {1:0.00} ({2:0.00} true) from {3},{4}, altitude {5}, deviation {6}", markType, bearing, fullBearing.CompassHeading, fullBearing.Location.Latitude.Value, fullBearing.Location.Longitude.Value, _state.AltitudeInMeters, _state.MagneticDeviation));

                    Mark mark;

                    if (!_state.Marks.Any(x=>x.MarkType==markType))
                    {
                        mark = new Mark() { MarkType = markType, CaptureMethod = MarkCaptureMethod.Bearing, Location = null };
                        mark.Bearings = new List<Bearing>();
                        mark.Bearings.Add(fullBearing);

                        State.Marks.Add(mark);
                        State.TargetMark = mark;
                    }
                    else
                    {
                        mark = _state.Marks.Where(x => x.MarkType == markType).Last();
                        mark.Bearings.Add(fullBearing);
                    }

                    if(mark.Bearings.Count>1 && mark.CaptureMethod == MarkCaptureMethod.Bearing)
                    {
                        var bearing1 = mark.Bearings[mark.Bearings.Count-2];
                        var bearing2 = mark.Bearings[mark.Bearings.Count - 1];

                        var location = CoordinatePointUtilities.FindIntersection(bearing1.Location, bearing1.CompassHeading, bearing2.Location, bearing2.CompassHeading);
                        mark.Location = location;

                        _logger.Info(string.Format("Calculated new location of {0} via bearings to be {1},{2}",markType,mark.Location.Latitude.Value,mark.Location.Longitude.Value));

                        //TODO, if there's more than 2, do we average down?
                    }
                }
            }
        }

        public void ClearMark(int markIndex)
        {
            throw new NotImplementedException();
        }

        public void NewRace()
        {
            throw new NotImplementedException();
        }
    }
}
