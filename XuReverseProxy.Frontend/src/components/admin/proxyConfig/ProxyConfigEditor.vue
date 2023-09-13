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
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";
import { ProxyConfigMode } from "@generated/Enums/Core/ProxyConfigMode";
import RadioButtonComponent from "@components/inputs/RadioButtonComponent.vue";
import { RadioButtonOption } from "@components/inputs/RadioButtonComponent.Models";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		DialogComponent,
		CheckboxComponent,
		CodeInputComponent,
		RadioButtonComponent
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

	modeOptions: Array<RadioButtonOption> = [
		{ label: 'Forward', value: ProxyConfigMode.Forward },
		{ label: 'Static HTML', value: ProxyConfigMode.StaticHTML }
	];

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

	get modeIsForward(): boolean { return this.localValue.mode == ProxyConfigMode.Forward; }
	get modeIsStaticHTML(): boolean { return this.localValue.mode == ProxyConfigMode.StaticHTML; }
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

		<!-- MODE SELECT -->
		<div class="mode-select mt-4">
			<radio-button-component label="Mode:"
				v-model:value="localValue.mode" :options="modeOptions" :disabled="disabled" />
		</div>

		<!-- MODE: FORWARD -->
		<div class="block block--dark mt-2" v-show="modeIsForward">
			<div class="block-title">Forward requests</div>
			<text-input-component label="Destination prefix" v-model:value="localValue.destinationPrefix" />
			<div class="forward-summary">
				<div class="icon-wrapper">
					<div class="material-icons icon">info</div>
				</div>
				<div class="forward-summary__content">
					<code><a :href="resultingProxyUrl">{{ resultingProxyUrl }}</a></code>
					<span>will forward to</span>
					<code>{{ localValue.destinationPrefix }}</code>
				</div>
			</div>
		</div>
		<!-- MODE: STATIC HTML -->
		<div class="block block--dark mt-2" v-show="modeIsStaticHTML">
			<div class="block-title">Serve static HTML</div>
			<code-input-component v-model:value="localValue.staticHTML" language="html" class="mt-2" height="400px" :readOnly="disabled" />
			<div class="forward-summary">
				<div class="icon-wrapper">
					<div class="material-icons icon">info</div>
				</div>
				<div class="forward-summary__content">
					<code><a :href="resultingProxyUrl">{{ resultingProxyUrl }}</a></code>
					<span>will serve the static HTML above.</span>
				</div>
			</div>
		</div>

		<div class="block block--dark mt-4">
			<div class="block-title">Challenge page</div>
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
		margin-top: 10px;
		overflow-x: auto;

		.icon-wrapper {
			align-self: center;
    		height: 24px;
		}
		.icon {
			color: var(--color--info-base);
			width: 24px;
			margin-right: 5px;
		}

		&__content {
			display: flex;
			align-items: center;
			font-size: 16px;
			padding: 10px;
			flex-wrap: wrap;

			code, span {
				margin-right: 5px;
			}
		}
	}
}
</style>
