<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import DialogComponent from "@components/common/DialogComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import { createProxyConfigResultingProxyUrl } from "@utils/ProxyConfigUtils";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		DialogComponent,
		CheckboxComponent
	}
})
export default class ProxyConfigEditor extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
  	@Prop()
	value: ProxyConfig;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyConfig | null = null;

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		const changed = JSON.stringify(this.localValue) != JSON.stringify(this.value);
		if (changed) this.localValue = JSON.parse(JSON.stringify(this.value));
    }

    @Watch('localValue', { deep: true })
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.updateLocalValue();
            return;
        }

		this.$emit('update:value', this.localValue);
    }

	get sudomainPlaceholder(): string {
		return `None, using root domain: ${this.options.rootDomain}`;
	}

	get resultingProxyUrl(): string {
		return createProxyConfigResultingProxyUrl(this.localValue, this.options.serverScheme, this.options.serverPort, this.options.serverDomain);
	}
}
</script>

<template>
	<div class="proxyconfig-edit" v-if="localValue">
		<checkbox-component label="Enabled" offLabel="Disabled" v-model:value="localValue.enabled" class="mt-1 mb-2" 
				:disabled="disabled" warnColorOff />
		<text-input-component label="Name" v-model:value="localValue.name" />
		<text-input-component label="Subdomain" v-model:value="localValue.subdomain" :placeholder="sudomainPlaceholder" />
		<text-input-component label="Listening port" v-model:value="localValue.port"
			type="number" :emptyIsNull="true" placeholder="Any" />
		<text-input-component label="Destination prefix" v-model:value="localValue.destinationPrefix" />

		<div class="forward-summary">
			<div class="material-icons icon">info</div>
			<code><a :href="resultingProxyUrl">{{ resultingProxyUrl }}</a></code>
			<span class="ml-2 mr-2">will forward to</span>
			<code>{{ localValue.destinationPrefix }}</code>
		</div>

		<div class="block-title mt-4">Challenge page</div>
		<div class="block block--dark">
			<text-input-component label="Title" v-model:value="localValue.challengeTitle" />
			<div class="input-wrapper">
				<label>Description</label>
				<textarea id="challengeDescription" v-model="localValue.challengeDescription" rows="3"></textarea>
			</div>
			<checkbox-component 
				label="Show completed challenges"
				offLabel="Hide completed challenges"
				:disabled="disabled"
				v-model:value="localValue.showCompletedChallenges" class="mt-2 mb-2" />
			<checkbox-component 
				label="Show challenges with unmet conditions"
				offLabel="Hide challenges with unmet conditions"
				:disabled="disabled"
				v-model:value="localValue.showChallengesWithUnmetRequirements" class="mt-2" />
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfig-edit {
	.forward-summary {
		display: flex;
		align-items: center;
		font-size: 16px;
		padding: 10px;
		margin-top: 10px;

		.icon {
			color: var(--color--info-base);
			width: 24px;
			margin-right: 5px;
		}
	}
}
</style>
