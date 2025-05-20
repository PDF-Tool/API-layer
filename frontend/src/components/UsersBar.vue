<script setup lang="ts">
import { useWebSocketStore } from '@/stores/webSocketStore'
import { toSvg } from "jdenticon";

const store = useWebSocketStore()

// Function to generate the jdenticon SVG data URL
const getUrl = (name: string) => {
  // Generate SVG string using jdenticon
  const svgString = toSvg(name, 40); // Smaller size might be better for w-10 h-10
  // Encode and create data URL
  return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svgString);
}
</script>

<template>
  <!-- Main container for the user bar -->
  <div class="flex items-center gap-4">

    <!-- Container for the overlapping user avatars -->
    <div class="flex">
      <!-- Loop through each user from the store -->
      <div
        v-for="(user, index) in store.users"
        :key="user"
        class="flex flex-col items-center"
        :style="{ marginLeft: index > 0 ? '-0.75rem' : '0' }"  
        >

        <!-- The circular avatar container -->
        <div
          class="bg-primary p-1 rounded-full w-10 h-10 flex items-center justify-center border-2 border-white relative hover:z-10 transition-transform hover:scale-110 cursor-default group"
          :class="{
            'ring-2 ring-offset-1 ring-offset-white ring-red-500': user === store.username
            // Apply red ring if the user matches the store's username
            // ring-offset-white helps the ring appear outside the white border
          }"
          :title="user" 
          ><!-- Add tooltip with username on hover -->

          <!-- The jdenticon image -->
          <img
            :src="getUrl(user)"
            alt="avatar"
            class="w-full h-full object-contain rounded-full" 
            ><!-- Ensure image itself is rounded if needed -->
        </div>

        <!-- "You" label, shown only for the current user -->
        <span
          v-if="user === store.username"
          class="mt-1 text-xs text-red-600 font-medium"
          > <!-- Adjust text color/style as needed -->
          You
        </span>
      </div>
    </div>

    <!-- Display the current user's name separately -->
    <span v-if="store.username" class="font-semibold text-black-100"> <!-- Adjust text color -->
      User: {{ store.username }}
    </span>

  </div>
</template>