<script lang="ts" setup>
import { ref } from 'vue';
import Progress from './Progress.vue';
import { Button } from '@/components/ui/button';

const progress = ref(50);

function incrementProgress() {
    // Increment by 10, but cap at 100
    progress.value = Math.min(progress.value + 10, 100);
}

function resetProgress() {
    progress.value = 0;
}
</script>

<template>
    <div>
        <div>
            <h2 class="font-medium">Queue</h2>
            <div class="flex flex-col gap-4">
                <Progress :progress="progress" title="Generated" />
            </div>
        </div>

        <div>
            <h2 class="font-medium ">Completed</h2>
            <div class="flex flex-col gap-4">
                <!-- Completed items would go here -->
            </div>
        </div>
    </div>
</template>