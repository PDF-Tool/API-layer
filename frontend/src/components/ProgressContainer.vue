<script lang="ts" setup>
import { computed } from 'vue';
import Progress from './Progress.vue';
import { useWebSocketStore } from '@/stores/webSocketStore';

const ws = useWebSocketStore();

const processList = computed(() => Object.values(ws.processes));
const queue = computed(() =>
  processList.value.filter(p => p.progress && !p.completed && !p.failed)
);
const completed = computed(() =>
  processList.value.filter(p => p.completed)
);

function formatDuration(duration: string) {
  const timeSpan = duration.split(':');
  const hours = parseInt(timeSpan[0]);
  const minutes = parseInt(timeSpan[1]);
  const seconds = parseInt(timeSpan[2]);

  if (hours > 0) {
    return `${hours}h ${minutes}m ${seconds}s`;
  } else if (minutes > 0) {
    return `${minutes}m ${seconds}s`;
  } else {
    return `${seconds}s`;
  }
}

function getCompletedStats(process: any) {
  const stats = [];
  
  if (process.completed?.Duration) {
    stats.push(`Total time: ${formatDuration(process.completed.Duration)}`);
  }
  
  if (process.completed?.AdditionalData) {
    const data = process.completed.AdditionalData;
    if (data.totalTasks !== undefined) {
      stats.push(`Completed: ${data.totalTasks}`);
    }
    if (data.errorCount !== undefined) {
      stats.push(`Errors: ${data.errorCount}`);
    }
    if (data.successCount !== undefined && data.totalFiles !== undefined) {
      stats.push(`Success: ${data.successCount}/${data.totalFiles}`);
    }
  }
  
  return stats.join(' | ');
}
</script>

<template>
  <div>
    <div>
      <h2 class="font-medium">Queue</h2>
      <div class="flex flex-col gap-4">
        <Progress v-for="p in queue" :key="p.progress?.ProcessId" :progress="p.progress?.PercentComplete || 0" :title="p.started?.ProcessName && p.started.ProcessName.includes('Random')
          ? `Random PDF Generation (by ${p.started?.Initiator || 'unknown'})`
          : p.started?.ProcessName && p.started.ProcessName.includes('Batch')
            ? `Batch PDF Generation (by ${p.started?.Initiator || 'unknown'})`
            : `${p.started?.ProcessName || 'PDF Generation'} (by ${p.started?.Initiator || 'unknown'})`"
          :current-stage="p.progress?.CurrentStage">
          <template #icon>
            <!-- <CanonLogo class="w-10 h-10 text-accent" /> -->
          </template>
        </Progress>
      </div>
    </div>

    <div>
      <h2 class="font-medium ">Completed</h2>
      <div class="flex flex-col gap-4">
        <Progress v-for="p in completed" :key="p.progress?.ProcessId" :progress="100" :title="p.started?.ProcessName && p.started.ProcessName.includes('Random')
          ? `Random PDF Generation (by ${p.started?.Initiator || 'unknown'})`
          : p.started?.ProcessName && p.started.ProcessName.includes('Batch')
            ? `Batch PDF Generation (by ${p.started?.Initiator || 'unknown'})`
            : `${p.started?.ProcessName || 'PDF Generation'} (by ${p.started?.Initiator || 'unknown'})`"
          :current-stage="getCompletedStats(p)">
          <template #icon>
            <!-- <CanonLogo class="w-10 h-10 text-accent" /> -->
          </template>
        </Progress>
      </div>
    </div>
  </div>
</template>