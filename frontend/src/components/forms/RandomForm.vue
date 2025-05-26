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
import { ref, computed } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'
import type { RandomPdfFields } from '@/types/pdf'
import Info from '@/components/info.vue'

const { createAndPrintRandomPdf } = usePdfStore()

const formData = ref<RandomPdfFields>({
    SizeMin: 1,
    SizeMax: 10,
    PageMin: 1,
    PageMax: 10,
    Mode: 'single',
    NumberOfFiles: 1,
    ByteUnit: 'MB',
    Host: '',
    SizePerPage: 1,
})

const canCreate = computed(() => {
    return formData.value.SizeMin > 0 &&
        formData.value.SizeMax >= formData.value.SizeMin &&
        formData.value.PageMin > 0 &&
        formData.value.PageMax >= formData.value.PageMin &&
        (formData.value.Mode === 'single' || formData.value.NumberOfFiles > 0) && formData.value.Host
})

function getRandomInt(min: number, max: number) {
    return Math.floor(Math.random() * (max - min + 1)) + min
}
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="grid w-full items-center gap-2">
            <Label for="mode">Generation Mode
                <Info text="Select whether to generate a single document or multiple documents." />
            </Label>
            <DropdownMenu>
                <DropdownMenuTrigger as-child>
                    <Button class="min-w-[120px] select-none">
                        {{ formData.Mode === 'single' ? 'Single Document' : 'Multiple Documents' }}
                        <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                    <DropdownMenuRadioGroup v-model="formData.Mode">
                        <DropdownMenuRadioItem value="single">Single Document</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="batch">Multiple Documents</DropdownMenuRadioItem>
                    </DropdownMenuRadioGroup>
                </DropdownMenuContent>
            </DropdownMenu>
        </div>
        <div class="grid w-full items-center gap-2" v-if="formData.Mode === 'batch'">
            <Label for="pages">Amount of documents
                <Info text="Select the amount of documents to generate." />
            </Label>
            <Input type="number" v-model="formData.NumberOfFiles" name="numberOfFiles" min="1"
                placeholder="Select amount of documents..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label>Size per page (random range)
                <Info
                    text="Select the amount of bytes per page in the PDF document, Select in the dropdown-menu in which size this needs to be." />
            </Label>
            <div class="flex gap-2">
                <Input type="number" v-model="formData.SizeMin" min="1" placeholder="Min size..." />
                <Input type="number" v-model="formData.SizeMax" min="1" placeholder="Max size..." />
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
            <Label>Pages (random range)</Label>
            <div class="flex gap-2">
                <Input type="number" v-model="formData.PageMin" min="1" placeholder="Min pages..." />
                <Input type="number" v-model="formData.PageMax" min="1" placeholder="Max pages..." />
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pages">Host/IP
                <Info text="Select the printer host address." />
            </Label>
            <Input type="text" name="Host" v-model="formData.Host" placeholder="Select host address..." />
        </div>
        <Button :disabled="!canCreate" @click="createAndPrintRandomPdf(formData)">
            Generate PDF(s) with Random Values
        </Button>
    </div>
</template>