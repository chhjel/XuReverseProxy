<script lang="ts">
import { LoadStatus } from "@services/ServiceBase";
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'

@Options({
    components: {  }
})
export default class LoadStatusComponent extends Vue {
    @Prop()
    status: LoadStatus | Array<LoadStatus>;
	
	mounted(): void {
	}

    get statuses(): Array<LoadStatus> {
        if (Array.isArray(this.status)) return this.status;
        return [this.status].filter(x => x != null);
    }

    get visibleStatuses(): Array<LoadStatus> {
        const statusShouldBeVisible = (status: LoadStatus): boolean => {
            return status.failed || status.inProgress;
        };

        return this.statuses.filter(x => statusShouldBeVisible(x));
    }

    get visibleStatus(): LoadStatus | null { return this.visibleStatuses[0]; }

    get allSuccess(): boolean {
        return this.visibleStatuses.every(x => x.done && !x.failed);
    }

    get visible(): boolean {
        return this.visibleStatus != null;
    }
}
</script>

<template>
    <div v-if="visible" class="load-status">
        <div class="load-statuses--inner">
            <div v-if="visibleStatus.statusText" class="load-status--text">{{ visibleStatus.statusText }}</div>
            <div class="load-status--error-wrapper"
                v-if="(visibleStatus.failed && visibleStatus.errorMessage) || (visibleStatus.failed && visibleStatus.errorDetails)">
                <div v-if="visibleStatus.failed && visibleStatus.errorMessage">> {{ visibleStatus.errorMessage }}</div>
                <code v-if="visibleStatus.failed && visibleStatus.errorDetails">{{ visibleStatus.errorDetails }}</code>
            </div>
        </div>
    </div>
</template>

<style scoped lang="scss">
.load-status {
    color: var(--color--error-base);
}
</style>
