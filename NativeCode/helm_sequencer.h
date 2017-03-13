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

      void getNoteEvents(Note** notes, event_map events, double start, double end);
      void getNoteOns(Note* notes[kMaxNotes], double start, double end);
      void getNoteOffs(Note* notes[kMaxNotes], double start, double end);
      double length() { return num_sixteenths_; }
      int channel() { return channel_; }
      void setLength(double length) { num_sixteenths_ = length; }
      void setChannel(int channel) { channel_ = channel; }

    private:
      int channel_;
      event_map on_events_;
      event_map off_events_;
      double num_sixteenths_;
  };

} // Helm

#endif // HELM_SEQUENCER_H
