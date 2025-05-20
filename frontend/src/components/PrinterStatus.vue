<script setup lang="ts">
import { useLprStore } from '@/stores/lprStore'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { computed, ref, onUnmounted, watch } from "vue";

const lprStore = useLprStore()

// --- Connection Class (Color) - Remains the same ---
const connectionClass = computed(() => {
    if (lprStore.isConnected === true) return 'text-green-600'
    if (lprStore.isConnected === false) return 'text-red-600'
    // Use yellow for null (unchecked) or when checking
    if (lprStore.isConnected === null || lprStore.isCheckingConnection) return 'text-yellow-600'
    return 'text-gray-500' // Default fallback (e.g., 'Disconnected')
})

// --- Animation driver - Remains the same ---
const tick = ref(0); // A simple ref to trigger updates for animation
let intervalId: number | undefined;

// Start/stop the interval based on isCheckingConnection - Remains the same
watch(() => lprStore.isCheckingConnection, (newValue) => {
  if (newValue) {
    if (intervalId) clearInterval(intervalId); // Clear previous interval if any
    intervalId = setInterval(() => {
      tick.value++; // Increment tick to trigger computed update
    }, 350); // Adjust speed of dot change (milliseconds)
  } else {
    if (intervalId) {
      clearInterval(intervalId);
      intervalId = undefined;
    }
    tick.value = 0; // Reset tick
  }
}, { immediate: true }); // Use immediate:true if you want the watch to run on component mount

// Clean up interval when the component is unmounted - Remains the same
onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId);
  }
});

// --- MODIFIED: Computed for the **status message text** ---
const displayStatusMessage = computed(() => {
    // --- Animate the status text itself when checking ---
    if (lprStore.isCheckingConnection) {
        const dots = ['.', '..', '...'];
        // Use the tick ref to ensure re-computation for animation
        return 'Connecting' + dots[tick.value % 3];
    }
    // --- Otherwise, show the static message from the store ---
    return lprStore.connectionStatusMessage;
});

</script>
<template>
    <!-- MODIFIED: Increased shadow from shadow-lg to shadow-xl -->
    <div class="fixed bottom-4 right-4 bg-card p-4 rounded-lg shadow-xl border w-80 z-50">
        <h3 class="text-lg font-semibold mb-3">PrinterServer Status:</h3>
        <div class="space-y-3">
            <div>
                <Label for="lprHostInput">Host/IP:</Label>
                <Input id="lprHostInput" v-model="lprStore.lprHost" placeholder="Printer Host or IP" @change="lprStore.saveSettings()" />
            </div>
            <div>
                <Label for="lprQueueInput">Queue:</Label>
                <Input id="lprQueueInput" v-model="lprStore.lprQueue" placeholder="Printer Queue Name" @change="lprStore.saveSettings()" />
            </div>
            <div>
                <Label for="lprPortInput">Port:</Label>
                <Input id="lprPortInput" type="number" v-model.number="lprStore.lprPort" placeholder="Port (e.g., 515)" @change="lprStore.saveSettings()" />
            </div>
            <div class="flex items-center justify-between pt-2">
                <span class="font-medium">Status:
                    <!-- Use the computed for color -->
                    <span :class="connectionClass">
                        <!-- MODIFIED: Bind status text to the (now animated) displayStatusMessage -->
                        {{ displayStatusMessage }}
                    </span>
                </span>
                <Button
                    @click="lprStore.checkLprConnection"
                    :disabled="lprStore.isCheckingConnection"
                    size="sm"
                    variant="outline"
                >
                    Connect
                </Button>
            </div>
        </div>
    </div>
</template>