export interface Run {
  id: string;
  instrumentId: string;
  sampleId: string;
  methodMetadataJson: string;
  currentState: RunState;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  actor: string | null;
  correlationId: string | null;
}

export type RunState = 'Created' | 'Queued' | 'Running' | 'Completed' | 'Failed' | 'Canceled';

export interface RunEvent {
  id: string;
  eventType: string;
  timestamp: string;
  data: string | null;
  actor: string | null;
  correlationId: string | null;
}

export interface RunTimeline {
  runId: string;
  events: RunEvent[];
}

export interface CreateRunRequest {
  instrumentId: string;
  sampleId: string;
  methodName?: string;
  methodVersion?: string;
  parameters?: Record<string, string>;
}

/** Matches API InstrumentHealthResponse from POST /instruments and GET /instruments/{id}/health. */
export interface InstrumentHealthResponse {
  instrumentId: string;
  name: string;
  status: string;
  lastHealthCheck: string | null;
  alarms: Alarm[];
}

export interface Alarm {
  id: string;
  severity: string;
  message: string;
  raisedAt: string;
  acknowledgedAt: string | null;
}

export interface CreateInstrumentRequest {
  name: string;
  type: string;
  serialNumber: string;
}

/**
 * Maps a RunState to the set of valid transition actions available from that state.
 */
export const STATE_TRANSITIONS: Record<RunState, string[]> = {
  Created: ['queue', 'cancel'],
  Queued: ['start', 'cancel'],
  Running: ['complete', 'fail', 'cancel'],
  Completed: [],
  Failed: [],
  Canceled: [],
};
