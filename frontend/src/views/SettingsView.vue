<script setup lang="ts">
import LpdServerConfig from '@/components/LpdServerConfig.vue'
import ConnectionStatusPanel from '@/components/ConnectionStatusPanel.vue'
</script>

<template>
  <div class="container py-6">
    <h1 class="text-2xl font-bold mb-6">Settings</h1>
    
    <div class="grid gap-6 md:grid-cols-2">
      <div class="space-y-6">
        <ConnectionStatusPanel />
        <LpdServerConfig />
      </div>
      
      <div class="space-y-6">
        <!-- Future settings panels can go here -->
      </div>
    </div>
  </div>
</template> 