/* Copyright 2017 Matt Tytel */

#include "helm_sequencer.h"

#define kDefaultNumSixteenths 16

namespace Helm {

  HelmSequencer::HelmSequencer() {
    channel_ = 0;
    start_time_ = 0.0f;
    num_sixteenths_ = kDefaultNumSixteenths;
  }

  HelmSequencer::~HelmSequencer() {
    for (auto note : on_events_)
      delete note.second;
    on_events_.clear();
    off_events_.clear();
  }

  HelmSequencer::Note* HelmSequencer::addNote(int midi_note, double velocity, double start, double end) {
    Note* note = new Note();
    note->midi_note = midi_note;
    note->velocity = velocity;
    note->time_on = start;
    note->time_off = end;

    on_events_[std::pair<double, int>(start, midi_note)] = note;
    off_events_[std::pair<double, int>(end, midi_note)] = note;
    return note;
  }

  void HelmSequencer::deleteNote(Note* note) {
    on_events_.erase(std::pair<double, int>(note->time_on, note->midi_note));
    off_events_.erase(std::pair<double, int>(note->time_off, note->midi_note));
    delete note;
  }

  void HelmSequencer::changeNoteStart(Note* note, double start) {
    on_events_.erase(std::pair<double, int>(note->time_on, note->midi_note));
    note->time_on = start;
    on_events_[std::pair<double, int>(start, note->midi_note)] = note;
  }

  void HelmSequencer::changeNoteEnd(Note* note, double end) {
    off_events_.erase(std::pair<double, int>(note->time_off, note->midi_note));
    note->time_off = end;
    off_events_[std::pair<double, int>(end, note->midi_note)] = note;
  }

  void HelmSequencer::changeNoteKey(Note* note, int midi_key) {
    on_events_.erase(std::pair<double, int>(note->time_on, note->midi_note));
    off_events_.erase(std::pair<double, int>(note->time_off, note->midi_note));
    note->midi_note = midi_key;
    on_events_[std::pair<double, int>(note->time_on, midi_key)] = note;
    off_events_[std::pair<double, int>(note->time_off, midi_key)] = note;
  }

  void HelmSequencer::getNoteEvents(Note** notes, event_map& events, double start, double end) {
    event_map::const_iterator iter = events.lower_bound(std::pair<double, int>(start, 0));

    int note_index = 0;
    while (iter != events.end() && (start > end || iter->first.first < end) && note_index < kMaxNotes) {
      notes[note_index++] = (*iter).second;
      iter++;
    }

    if (start > end) {
      iter = events.lower_bound(std::pair<double, int>(0.0, 0));

      while (iter != events.end() && iter->first.first < end && note_index < kMaxNotes) {
        notes[note_index++] = (*iter).second;
        iter++;
      }
    }

    notes[note_index] = nullptr;
  }

  void HelmSequencer::getNoteOns(Note** notes, double start, double end) {
    getNoteEvents(notes, on_events_, start, end);
  }

  void HelmSequencer::getNoteOffs(Note** notes, double start, double end) {
    getNoteEvents(notes, off_events_, start, end);
  }
}
