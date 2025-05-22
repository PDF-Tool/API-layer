<script setup lang="ts">
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger
} from '@/components/ui/tooltip'

defineProps<{
    text: string
}>()
</script>

<template>
    <TooltipProvider>
        <Tooltip>
            <TooltipTrigger>
                <span
                    class="inline-flex items-center justify-center rounded-full bg-primary  text-accent-foreground w-4 h-4 text-xs font-semibold">i</span>
            </TooltipTrigger>
            <TooltipContent>
                <p>{{ text }}</p>
            </TooltipContent>
        </Tooltip>
    </TooltipProvider>
</template>