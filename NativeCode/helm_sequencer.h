/* Copyright 2017 Matt Tytel */

#pragma once
#ifndef HELM_SEQUENCER_H
#define HELM_SEQUENCER_H

#include <map>

namespace Helm {

  class HelmSequencer {
    public:
      struct Note {
        int midi_note;
        double velocity;
        double time_on;
        double time_off;
      };

      typedef std::map<std::pair<double, int>, Note*> event_map;

      const static int kMaxNotes = 127;

      HelmSequencer();
      virtual ~HelmSequencer();

      Note* addNote(int midi_note, double velocity, double start, double end);
      void deleteNote(Note* note);
      void changeNoteStart(Note* note, double start);
      void changeNoteEnd(Note* note, double end);
      void changeNoteKey(Note* note, int midi_key);

      void getNoteEvents(Note** notes, event_map& events, double start, double end);
      void getNoteOns(Note* notes[kMaxNotes], double start, double end);
      void getNoteOffs(Note* notes[kMaxNotes], double start, double end);
      double length() { return num_sixteenths_; }
      int channel() { return channel_; }
      double start_time() { return start_time_; }
      void setLength(double length) { num_sixteenths_ = length; }
      void setChannel(int channel) { channel_ = channel; }

      void armStartTime(double wait_time) {
        start_time_armed_ = true;
        wait_time_ = wait_time;
      }

      void trySetStartTime(double time) {
        if (start_time_armed_)
          start_time_ = time + wait_time_;
        start_time_armed_ = false;
      }

      void shiftStartTime(double time) {
        start_time_ += time;
      }

    private:
      int channel_;
      event_map on_events_;
      event_map off_events_;
      double num_sixteenths_;
      double start_time_;
      bool start_time_armed_;
      double wait_time_;
  };

} // Helm

#endif // HELM_SEQUENCER_H
