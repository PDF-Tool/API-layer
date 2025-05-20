<script setup lang="ts">
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuRadioGroup,
    DropdownMenuRadioItem,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { fileSizeFormats, metrics } from '@/lib/utils'
import { pageFormats } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'
import { computed } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'

const { formData, createAndPrintPdf } = usePdfStore()

const canCreatePdf = computed(() => {
    return formData.Pages > 0 && formData.SizePerPage > 0
})
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="grid w-full items-center gap-2">
            <TooltipProvider>
                <Tooltip>
                    <TooltipTrigger>
                        <Label for="size">Size per page</Label>
                    </TooltipTrigger>
                    <TooltipContent>
                        <p>Select the amount of bytes per page in the PDF document, Select in the dropdown-menu in which size this needs to be.</p>
                    </TooltipContent>
                </Tooltip>
            </TooltipProvider>
            <div class="flex gap-2">
                <Input type="number" v-model="formData.SizePerPage" name="sizePerPage" placeholder="Select image size per page..." />
                <DropdownMenu>
                    <DropdownMenuTrigger as-child>
                        <Button class="min-w-[70px] select-none"> {{ formData.ByteUnit }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuRadioGroup v-model="formData.ByteUnit">
                            <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
                        <TooltipProvider>
                <Tooltip>
                    <TooltipTrigger>
                        <Label for="size">Amount of pages</Label>
                    </TooltipTrigger>
                    <TooltipContent>
                        <p>Select the amount of pages you want the PDF document to be.</p>
                    </TooltipContent>
                </Tooltip>
            </TooltipProvider>
            <Input type="number" name="pages" v-model="formData.Pages" placeholder="Select amount of pages..." />
        </div>
        <div class="p-4 bg-primary rounded-lg border-input border shadow-sm flex flex-col gap-4">
            <div class="flex flex-col w-full items-center gap-2">
                <label class="w-full">Preset</label>
                <DropdownMenu class="w-full">
                    <DropdownMenuTrigger as-child>
                        <Button :disabled="!!formData.Width || !!formData.Height"
                            class="select-none border-1 text-black  bg-background! justify-start hover:bg-transparent  w-full">
                            {{ formData.Format }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent class="w-full">
                        <DropdownMenuRadioGroup v-model="formData.Format">
                            <DropdownMenuRadioItem v-for="format in pageFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
            <div class="grid w-full items-center gap-2">
                <Label for="size">Dimensions</Label>
                <div class="flex gap-2">
                    <div class="relative">
                        <Input id="size" type="number" v-model="formData.Width" class="pl-7 bg-background!" />
                        <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                            <span class="text-foreground/50">W</span>
                        </span>
                    </div>
                    <div class="relative">
                        <Input id="size" type="number" v-model="formData.Height" class="pl-7 bg-background!" />
                        <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                            <span class="text-foreground/50">H</span>
                        </span>
                    </div>
                    <DropdownMenu>
                        <DropdownMenuTrigger as-child>
                            <Button class="min-w-[70px] select-none"> {{ formData.MetricUnit }}
                                <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent>
                            <DropdownMenuRadioGroup v-model="formData.MetricUnit">
                                <DropdownMenuRadioItem v-for="metric in metrics" :key="metric" :value="metric">
                                    {{ metric }}
                                </DropdownMenuRadioItem>
                            </DropdownMenuRadioGroup>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            </div>
        </div>
        <!-- <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-2">
                <Label for="checkHost">Check host connection</Label>
                <div class="flex gap-2">
                    <Input id="checkHost" name="checkHost" type="text" placeholder="Enter host address..." />
                    <Button>
                        <span>Check</span>
                    </Button>
                </div>
            </div>
        </div> -->
        <Button :disabled="!canCreatePdf" @click="createAndPrintPdf">Generate PDF</Button>
    </div>
</template>