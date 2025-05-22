<script setup lang="ts">
import { useLprStore } from '@/stores/lprStore'
import { RouterLink, RouterView } from 'vue-router'
import { useWebSocketStore } from '@/stores/webSocketStore'
import { ref, onMounted } from 'vue'
import UsersBar from '@/components/UsersBar.vue'
import ProgressContainer from './components/ProgressContainer.vue'
import PrinterStatus from './components/PrinterStatus.vue'
import { Toaster } from 'vue-sonner'

const websocketStore = useWebSocketStore()
const isConnecting = ref(false)



onMounted(() => {
  websocketStore.connect("Canon" + Math.floor(Math.random() * 1000))
  isConnecting.value = true
})
</script>

<template>
  <Toaster richColors />
  <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
    <header class="pb-8 flex justify-between items-center">
      <img alt="Canon logo" src="./assets/canon.svg" width="190" height="125" />
      <users-bar />
    </header>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-6 lg:gap-8">
      <div class="w-full">
        <RouterView />
      </div>
      <div class="w-full">
        <ProgressContainer />
      </div>
    </div>
  </main>
</template>