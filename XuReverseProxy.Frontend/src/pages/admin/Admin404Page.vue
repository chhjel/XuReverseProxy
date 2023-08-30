<script lang="ts">
import { Options } from "vue-class-component";
import { Vue } from 'vue-property-decorator'

@Options({
	components: {}
})
export default class Admin404Page extends Vue { }
</script>

<template>
	<div class="error-page">
		<div class="error-page__box">
			<div class="error-page__code">404</div>
			<div class="error-page__note">Seems this page got lost somewhere..</div>
			<a class="error-page__link" href="/">Back to admin overview</a>
		</div>
	</div>
</template>

<style scoped lang="scss">
.error-page {
	padding-top: 20%;

	&__box {
		width: calc(max(50%, 200px));
		margin: auto;
		border: 2px solid var(--color--panel);
		padding: 20px;
		text-align: center;
	}

	&__note {
		margin-bottom: 20px;
		color: var(--color--warning-base);
		font-size: 15px;
	}

	&__code {
		font-size: 12vw;
	}
}
</style>
